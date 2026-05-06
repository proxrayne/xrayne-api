using System.CommandLine;
using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using XRayne.Cli.Commands;
using XRayne.Core;
using XRayne.Infrastructure;
using XRayne.Infrastructure.Values;
using XRayne.Repositories;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var host = Host.CreateDefaultBuilder(args);

    host.ConfigureAppConfiguration((context, configuration) =>
    {
        configuration.SetBasePath(AppContext.BaseDirectory);
        configuration.AddJsonFile("config.json", optional: true, reloadOnChange: true);
        configuration.AddJsonFile(
            $"config.{context.HostingEnvironment.EnvironmentName}.json",
            optional: true,
            reloadOnChange: true);
        configuration.AddJsonFile(PathProvider.Paths.JsonConfig, optional: true, reloadOnChange: true);
        configuration.AddEnvFile(PathProvider.Paths.EnvConfig, optional: true);

        configuration.AddEnvironmentVariables();
    });

    host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Map(
            logEvent => logEvent.Timestamp.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            (date, writeTo) => writeTo.File(
                path: $"logs/cli-{date}.log",
                shared: true,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}"),
            sinkMapCountLimit: 1));

    host.ConfigureServices((context, services) =>
    {
        services.AddCoreDependencies(context.Configuration);
        services.AddInfrastructure(context.Configuration);
        services.AddRepositories(context.Configuration);
        services.AddCliActions();
    });

    using var app = host.Build();

    Log.Information("XRayne CLI started.");

    var rootCommand = app.Services.GetRequiredService<RootCommandFactory>().Create();

    var configuration = new CommandLineConfiguration(rootCommand);

    return await configuration.InvokeAsync(args);
}
catch (Exception exception)
{
    Log.Fatal(exception, "XRayne CLI terminated unexpectedly.");

    return 1;
}
finally
{
    Log.CloseAndFlush();
}
