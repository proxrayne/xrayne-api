using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using XRayne.Core;
using XRayne.Infrastructure;
using XRayne.Repositories;

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(configuration =>
    {
        configuration.AddJsonFile("appsettings.json", optional: true);
        configuration.AddEnvironmentVariables("XRAYNE_");
    })
    .ConfigureServices((context, services) =>
    {
        services.AddCoreDependencies(context.Configuration);
        services.AddInfrastructure(context.Configuration);
        services.AddRepositories(context.Configuration);
    })
    .Build();

Console.WriteLine("XRayne CLI");
return 0;
