using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using WorkerService;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((services) =>
    {
        services.AddHostedService<Worker>();
        string CONNSTR = System.Configuration.ConfigurationManager.ConnectionStrings["AzureBusConnection"].ConnectionString;

        services.AddSingleton((s) =>
        {
            return new ServiceBusClient(CONNSTR, new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets });
        });
        services.AddSingleton<IAzBusService, AzBusService>();
    })
    .UseWindowsService()
    .Build();

await host.RunAsync();
