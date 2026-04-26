using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using XRayne.Api.Auth;
using XRayne.Core;
using XRayne.Infrastructure;
using XRayne.Infrastructure.Auth;
using XRayne.Repositories;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    var IsDocsEnabled = builder.Configuration.GetValue("Docs", false);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Map(
            logEvent => logEvent.Timestamp.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            (date, writeTo) => writeTo.File(
                path: $"logs/api-{date}.log",
                shared: true,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}"),
            sinkMapCountLimit: 1));

    builder.Services.AddControllers();
    builder.Services.AddAutoMapper(typeof(Program).Assembly);

    if (IsDocsEnabled)
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddOpenApi();
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
    builder.Services.AddRepositories(builder.Configuration);

    var app = builder.Build();

    await app.Services.MigrateDatabaseAsync();

    app.UseSerilogRequestLogging();

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
