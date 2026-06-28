using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using Serilog;
using XRayne.Node.Security;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    var isDocsEnabled = builder.Configuration.GetValue("Docs", false);
    var nodeApiKey = builder.Configuration["Node:ApiKey"];

    if (!builder.Environment.IsDevelopment() && string.IsNullOrWhiteSpace(nodeApiKey))
    {
        throw new InvalidOperationException("Node:ApiKey must be configured outside Development.");
    }

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Map(
            logEvent => logEvent.Timestamp.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            (date, writeTo) => writeTo.File(
                path: Path.Combine("logs", $"node-{date}.log"),
                shared: true,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}"),
            sinkMapCountLimit: 1));

    builder.Services.AddControllers().AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    });

    if (isDocsEnabled)
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes[ApiKeyAuthentication.ApiKeySchemeName] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.ApiKey,
                    In = ParameterLocation.Header,
                    Name = ApiKeyAuthentication.HeaderName,
                    Description = "Node API key."
                };

                document.SecurityRequirements.Add(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = ApiKeyAuthentication.ApiKeySchemeName
                        }
                    }] = []
                });

                return Task.CompletedTask;
            });
        });
    }

    var app = builder.Build();

    app.UseSerilogRequestLogging();

    if (isDocsEnabled)
    {
        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options.Title = "XRayne Node API";
            options.Theme = ScalarTheme.Default;
            options.OperationTitleSource = OperationTitleSource.Summary;
        });
    }

    app.UseMiddleware<ApiKeyAuthenticationMiddleware>();

    app.MapControllers();

    app.Run();
}
catch (Exception exception)
{
    Log.Fatal(exception, "XRayne Node terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program;
