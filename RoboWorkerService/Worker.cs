using RoboWorkerService.Interface;
using RoboWorkerService.Interfaces;
using RoboWorkerService.Market.Processing;

namespace RoboWorkerService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IMarketCoreCupBroker<ICryptoALGO> _marketCoreAlgocCupBroker;
    private readonly IMarketCoreCupBroker<ICryptoBTC> _marketCoreBtcCupBroker;
    private readonly IMarketCoreDefinedMoneyBroker<ICryptoBTC> _marketCoreBtcSharpBroker;
    private readonly IMarketCoreCupBroker<ICryptoDOGE> _marketCoreDogeCupBroker;
    private readonly IMarketCoreCupBroker<ICryptoETH> _marketCoreEthCupBroker;

    public Worker(
        ILogger<Worker> logger,
        IMarketCoreCupBroker<ICryptoBTC> marketCoreBTC_CupBroker,
        IMarketCoreDefinedMoneyBroker<ICryptoBTC> marketCoreBTCSharpBroker,
        IMarketCoreCupBroker<ICryptoALGO> marketCoreALGOC_CupBroker,
        IMarketCoreCupBroker<ICryptoETH> marketCoreETH_CupBroker,
        IMarketCoreCupBroker<ICryptoDOGE> marketCoreDOGE_CupBroker
    )
    {
        _logger = logger;
        _marketCoreBtcCupBroker = marketCoreBTC_CupBroker;
        _marketCoreBtcSharpBroker = marketCoreBTCSharpBroker;
        _marketCoreAlgocCupBroker = marketCoreALGOC_CupBroker;
        _marketCoreEthCupBroker = marketCoreETH_CupBroker;
        _marketCoreDogeCupBroker = marketCoreDOGE_CupBroker;
    }
    // coinmate - CZ

    //coinbase

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("-*CONNECT TO MARKET*-");
        await _marketCoreBtcCupBroker.ConnectToMarketAsync();
        await _marketCoreBtcSharpBroker.ConnectToMarketAsync();

        await _marketCoreAlgocCupBroker.ConnectToMarketAsync();
        await _marketCoreEthCupBroker.ConnectToMarketAsync();
        await _marketCoreDogeCupBroker.ConnectToMarketAsync();
        _logger.LogInformation("-*START ROBO STRATEGY*-");
        var delay = 3000;
        while (!stoppingToken.IsCancellationRequested)
            try
            {
                await _marketCoreBtcCupBroker.RunAsync();
                await Task.Delay(delay, stoppingToken);
                await _marketCoreBtcSharpBroker.RunAsync();
                await Task.Delay(delay, stoppingToken);
                await _marketCoreAlgocCupBroker.RunAsync();
                await Task.Delay(delay, stoppingToken);
                await _marketCoreEthCupBroker.RunAsync();
                await Task.Delay(delay, stoppingToken);
                await _marketCoreDogeCupBroker.RunAsync();
                await Task.Delay(delay, stoppingToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Worker running at: {time}", DateTimeOffset.Now);
                throw;
            }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        return base.StopAsync(cancellationToken);
    }
}