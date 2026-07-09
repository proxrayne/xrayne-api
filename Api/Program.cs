using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Api.Auth;
using Api.Filters;
using Contracts;
using Contracts.Configurations;
using Contracts.Values;
using Infrastructure;
using Infrastructure.Services;
using Infrastructure.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Quartz;
using RemoteNode;
using RemoteNode.Configurations;
using Data;
using Scalar.AspNetCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Configuration.AddJsonFile(PathProvider.Paths.JsonConfig, optional: true, reloadOnChange: true);
    builder.Configuration.AddEnvFile(PathProvider.Paths.EnvConfig, optional: true);

    var IsDocsEnabled = builder.Configuration.GetValue("Docs", false);
    var devSpaOrigins = builder.Configuration.GetSection("Cors:SpaOrigins").Get<string[]>() ?? [];

    var settings = PanelSettings.Parse(BuildPanelBootstrapConfiguration());
    if (!builder.Environment.IsDevelopment() && PanelStartupReader.ShouldOverrideKestrel(settings))
    {
        using var bootstrapLoggerFactory = LoggerFactory.Create(b => b.AddSerilog());
        var kestrelLogger = bootstrapLoggerFactory.CreateLogger("Kestrel");

        builder.WebHost.ConfigureKestrel(o => PanelStartupReader.ApplyKestrel(o, settings, kestrelLogger));
    }

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Map(
            logEvent => logEvent.Timestamp.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            (date, writeTo) => writeTo.File(
                path: Path.Combine("logs", $"api-{date}.log"),
                shared: true,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}"),
            sinkMapCountLimit: 1));

    builder.Services.AddMemoryCache();
    builder.Services.AddControllers(options =>
    {
        options.Filters.Add<ApiExceptionFilter>();
    }).AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    });
    builder.Services.AddAutoMapper(typeof(Program).Assembly);

    var allowedOrigins = BuildAllowedOrigins(devSpaOrigins, settings.Domain);

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("SpaClient", policy =>
        {
            policy
                .WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    });

    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor
            | ForwardedHeaders.XForwardedProto
            | ForwardedHeaders.XForwardedHost;

        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();

    });

    if (IsDocsEnabled)
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "JWT Authorization header using the Bearer scheme."
                };

                document.SecurityRequirements.Add(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    }] = []
                });

                return Task.CompletedTask;
            });
        });
    }


    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()!;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidAudience = jwtOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret))
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var isEventStreamRequest = context.Request.Headers.Accept.Any(value => value?.Contains("text/event-stream", StringComparison.OrdinalIgnoreCase) == true);
                    if (!string.IsNullOrWhiteSpace(accessToken) && isEventStreamRequest)
                    {
                        context.Token = accessToken;
                    }

                    return Task.CompletedTask;
                }
            };

            options.Events.OnTokenValidated = AdminJwtValidation.ValidateActiveAdminAsync;
        });

    builder.Services.AddAuthorization(options =>
    {
        options.AddAdminPermissionPolicies();
    });

    builder.Services.AddSingleton(settings);
    builder.Services.AddRemoteNodes(new RemoteNodeOptions
    {
        PingTimeoutSeconds = builder.Configuration.GetValue("NodeConnection:PingTimeoutSeconds", 15)
    });
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddData(builder.Configuration.GetConnectionString("Default"));
    builder.Services.AddContracts(builder.Configuration);
    builder.Services.AddSingleton<IPanelRestartService, PanelRestartService>();

    builder.Services.AddQuartz(options =>
    {
        options.SchedulerName = "xrayne-scheduler";
        options.SchedulerId = "xrayne-api";

        options.UseInMemoryStore();
        options.UseDefaultThreadPool(threadPool =>
        {
            threadPool.MaxConcurrency = 5;
        });

        options.AddJob<GeoResourceSyncJob>(job => job.WithIdentity(GeoResourceSyncJob.JobKey));
        options.AddTrigger(trigger => trigger
            .WithIdentity(GeoResourceSyncJob.TriggerKey)
            .ForJob(GeoResourceSyncJob.JobKey)
            .StartNow()
            .WithSimpleSchedule(schedule => schedule
                .WithInterval(TimeSpan.FromHours(2))
                .RepeatForever()));

        options.AddJob<GeoResourceAutoUpdateJob>(job => job.WithIdentity(GeoResourceAutoUpdateJob.JobKey));
        options.AddTrigger(trigger => trigger
            .WithIdentity(GeoResourceAutoUpdateJob.TriggerKey)
            .ForJob(GeoResourceAutoUpdateJob.JobKey)
            .StartNow()
            .WithSimpleSchedule(schedule => schedule
                .WithInterval(TimeSpan.FromMinutes(10))
                .RepeatForever()));
    });
    builder.Services.AddQuartzHostedService(options =>
    {
        options.WaitForJobsToComplete = true;
    });

    var app = builder.Build();

    await app.Services.MigrateDatabaseAsync();

    app.UseSerilogRequestLogging();
    app.UseForwardedHeaders();

    if (IsDocsEnabled)
    {
        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options.Title = "XRayne API";
            options.Theme = ScalarTheme.Default;
            options.OperationTitleSource = OperationTitleSource.Summary;
        });
    }

    app.UseCors("SpaClient");

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (HostAbortedException)
{
    // EF Core tools abort the host after resolving the application's services at design time.
}
catch (Exception exception)
{
    Log.Fatal(exception, "XRayne API terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}


static string[] BuildAllowedOrigins(string[] devOrigins, string? domain)
{
    if (string.IsNullOrWhiteSpace(domain))
    {
        return devOrigins;
    }

    var normalized = domain.Contains("://", StringComparison.Ordinal)
        ? domain.TrimEnd('/')
        : $"https://{domain.TrimEnd('/')}";

    return [.. devOrigins, normalized];
}

static IConfiguration BuildPanelBootstrapConfiguration()
{
    return new ConfigurationBuilder()
        .AddEnvFile(PathProvider.Paths.EnvConfig, optional: true)
        .AddEnvironmentVariables()
        .Build();
}

public partial class Program;
