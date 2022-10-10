using Newtonsoft.Json;
using RoboWorkerService.Interface;
using RoboWorkerService.Market.Enum;
using RoboWorkerService.Market.Model;

namespace RoboWorkerService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IMarketProcessing _marketProcessing;
    private readonly IWalletMarketTransactionData _walletMarketTransactionData;

    public Worker(ILogger<Worker> logger, IMarketProcessing marketProcessing, IWalletMarketTransactionData walletMarketTransactionData)
    {
        _logger = logger;
        _marketProcessing = marketProcessing;
        _walletMarketTransactionData = walletMarketTransactionData;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _marketProcessing.InitAsync(CryptoCurrencyDefinitionList.BTC_EUR);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _marketProcessing.RunAsync();


                await Task.Delay(5000, stoppingToken);

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
