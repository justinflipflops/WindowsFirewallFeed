using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WindowsFirewallFeed.Services
{
    public class FeedSvc : BackgroundService
    {
        public FeedSvc(ILoggerFactory loggerFactory)
        {
            Logger = loggerFactory.CreateLogger<FeedSvc>();
        }

        public ILogger Logger { get; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Logger.LogInformation("Windows Defender Firewall Feed Service is starting.");

            stoppingToken.Register(() => Logger.LogInformation("Windows Defender Firewall Feed Service is stopping."));

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }

            Logger.LogInformation("Windows Defender Firewall Feed Service has stopped.");
        }
    }
}
