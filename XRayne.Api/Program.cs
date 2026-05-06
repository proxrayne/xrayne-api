using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using Serilog;
using XRayne.Api.Auth;
using XRayne.Api.Filters;
using XRayne.Core;
using XRayne.Infrastructure;
using XRayne.Infrastructure.Auth;
using XRayne.Infrastructure.Values;
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
    var allowedSpaOrigins = builder.Configuration.GetSection("Cors:SpaOrigins").Get<string[]>()
        ?? [];

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Map(
            logEvent => logEvent.Timestamp.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            (date, writeTo) => writeTo.File(
                path:  Path.Combine("logs", $"api-{date}.log"),
                shared: true,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}"),
            sinkMapCountLimit: 1));

    builder.Services.AddControllers(options =>
    {
        options.Filters.Add<ApiExceptionFilter>();
    });
    builder.Services.AddAutoMapper(typeof(Program).Assembly);
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("SpaClient", policy =>
        {
            policy
                .WithOrigins(allowedSpaOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
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

    var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()!;

    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
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
        });

    builder.Services.AddAuthorization(options =>
    {
        options.AddAdminPermissionPolicies();
    });

    builder.Services.AddCoreDependencies(builder.Configuration);
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddRepositories(builder.Configuration.GetConnectionString("Default"));

    var app = builder.Build();
    var pathBase = NormalizePathBase(app.Configuration["PathBase"]);

    await app.Services.MigrateDatabaseAsync();

    app.UseSerilogRequestLogging();

    if (!string.IsNullOrWhiteSpace(pathBase))
    {
        app.UsePathBase(pathBase);
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

static string NormalizePathBase(string? value)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        return string.Empty;
    }

    var pathBase = value.Trim().Trim('/');

    return pathBase.Length == 0
        ? string.Empty
        : $"/{pathBase}";
}
