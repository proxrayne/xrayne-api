using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        services.AddXRayneCore();
        services.AddXRayneInfrastructure(context.Configuration);
        services.AddXRayneRepositories(context.Configuration);
    })
    .Build();

Console.WriteLine("XRayne CLI");
return 0;
