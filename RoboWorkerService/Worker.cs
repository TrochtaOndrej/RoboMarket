using Newtonsoft.Json;
using RoboWorkerService.Interface;
using RoboWorkerService.Market;
using RoboWorkerService.Market.Enum;
using RoboWorkerService.Market.Model;

namespace RoboWorkerService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IMarketCoreCupBroker<ICryptoBTC> _marketCoreCupBroker;

    public Worker(
        ILogger<Worker> logger,
        IMarketCoreCupBroker<ICryptoBTC> marketCoreCupBroker)
    {
        _logger = logger;
        _marketCoreCupBroker = marketCoreCupBroker;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _marketCoreCupBroker.ConnectToMarketAsync();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _marketCoreCupBroker.RunAsync();

                await Task.Delay(500, stoppingToken);

            }
            catch (Exception e)
            {
                _logger.LogError(e, "Worker running at: {time}", DateTimeOffset.Now);
                throw;
            }
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
       
        return base.StopAsync(cancellationToken);
    }
}
