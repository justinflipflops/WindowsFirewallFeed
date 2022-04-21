using Microsoft.Extensions.Hosting.WindowsServices;
using FirewallFeedService;

var options = new WebApplicationOptions

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
