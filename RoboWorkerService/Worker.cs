using Newtonsoft.Json;
using RoboWorkerService.Interface;
using RoboWorkerService.Market;
using RoboWorkerService.Market.Enum;

namespace RoboWorkerService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IMarketCoreCupBroker<ICryptoBTC> _marketCoreBTCCupBroker;
    private readonly IMarketCoreCupBroker<ICryptoALGO> _marketCoreAlgocCupBroker;
    private readonly IMarketCoreCupBroker<ICryptoETH> _marketCoreEthCupBroker;
    private readonly IMarketCoreCupBroker<ICryptoDOGE> _marketCoreDogeCupBroker;

    public Worker(
        ILogger<Worker> logger,
        IMarketCoreCupBroker<ICryptoBTC> marketCoreBTC_CupBroker,
        IMarketCoreCupBroker<ICryptoALGO> marketCoreALGOC_CupBroker,
        IMarketCoreCupBroker<ICryptoETH> marketCoreETH_CupBroker,
        IMarketCoreCupBroker<ICryptoDOGE> marketCoreDOGE_CupBroker
        )
    {
        _logger = logger;
        _marketCoreBTCCupBroker = marketCoreBTC_CupBroker;
        _marketCoreAlgocCupBroker = marketCoreALGOC_CupBroker;
        _marketCoreEthCupBroker = marketCoreETH_CupBroker;
        _marketCoreDogeCupBroker = marketCoreDOGE_CupBroker;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _marketCoreBTCCupBroker.ConnectToMarketAsync();
        await _marketCoreAlgocCupBroker.ConnectToMarketAsync();
        await _marketCoreEthCupBroker.ConnectToMarketAsync();
        await _marketCoreDogeCupBroker.ConnectToMarketAsync();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _marketCoreBTCCupBroker.RunAsync();
                await _marketCoreAlgocCupBroker.RunAsync();
                await _marketCoreEthCupBroker.RunAsync();
                await _marketCoreDogeCupBroker.RunAsync();
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
