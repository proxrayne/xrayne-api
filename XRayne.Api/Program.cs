using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Quartz;
using Scalar.AspNetCore;
using Serilog;
using XRayne.Api.Auth;
using XRayne.Api.Filters;
using XRayne.Contracts;
using XRayne.Contracts.Configurations;
using XRayne.Contracts.Values;
using XRayne.Infrastructure;
using XRayne.Infrastructure.Services;
using XRayne.Repositories;

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

    PanelSettings settings = PanelSettings.Parse(builder.Configuration);
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

        // ApplyTrustedProxyCidrs(options, settings.TrustedProxyCidrs);
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
        });

    builder.Services.AddAuthorization(options =>
    {
        options.AddAdminPermissionPolicies();
    });

    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddRepositories(builder.Configuration.GetConnectionString("Default"));
    builder.Services.AddContracts(builder.Configuration);
    builder.Services.AddSingleton<IPanelRestartService, PanelRestartService>();

    if (settings.SessionLifetimeMinutes > 0)
    {
        builder.Services.Configure<JwtOptions>(o =>
            o.AccessTokenLifetimeMinutes = settings.SessionLifetimeMinutes);
    }

    builder.Services.AddQuartz(options =>
    {
        options.SchedulerName = "XRayneScheduler";
        options.SchedulerId = "XRayneApi";

        options.UseInMemoryStore();
        options.UseDefaultThreadPool(threadPool =>
        {
            threadPool.MaxConcurrency = 5;
        });
    });
    builder.Services.AddQuartzHostedService(options =>
    {
        options.WaitForJobsToComplete = true;
    });

    var app = builder.Build();

    await app.Services.MigrateDatabaseAsync();

    app.UseSerilogRequestLogging();
    app.UseForwardedHeaders();

    if (!string.IsNullOrWhiteSpace(settings.PathBase))
    {
        app.UsePathBase(settings.PathBase);
    }

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


    app.UseDefaultFiles();
    app.UseStaticFiles();

    app.UseCors("SpaClient");

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapFallback("{*path:nonfile}", async context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;

            return;
        }

        var indexPath = Path.Combine(app.Environment.WebRootPath, "index.html");

        context.Response.ContentType = "text/html; charset=utf-8";

        await context.Response.SendFileAsync(indexPath);
    });

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

/* static void ApplyTrustedProxyCidrs(ForwardedHeadersOptions options, string? cidrs)
{
    if (string.IsNullOrWhiteSpace(cidrs))
    {
        return;
    }

    foreach (var entry in cidrs.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
    {
        if (System.Net.IPNetwork.TryParse(entry, out var parsed))
        {
            options.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(parsed.BaseAddress, parsed.PrefixLength));
            continue;
        }

        if (IPAddress.TryParse(entry, out var ip))
        {
            options.KnownProxies.Add(ip);
        }
    }
} */

public partial class Program;
