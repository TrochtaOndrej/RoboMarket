using RoboWorkerService.Interface;
using RoboWorkerService.Interfaces;
using RoboWorkerService.Market.Processing;

namespace RoboWorkerService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private static int _counter = 0;
    private static IAppRobo _appRobo;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        List<Type> typeCryptoOnMarket = new List<Type>()
        {
            typeof(ICryptoBTC)
            /*, typeof(ICryptoETH) */
        };

        List<IMarketCoreBroker> cryptoProcessing = new List<IMarketCoreBroker>();

        foreach (var crypto in typeCryptoOnMarket)
        {
            var marketCoreCrypto =
                (IMarketCoreBroker)HostApp.Host.Services.GetService(
                    typeof(IMarketCoreSharpBroker<>).MakeGenericType(new[] { crypto }))!;
            cryptoProcessing.Add(marketCoreCrypto);
        }

        _logger.LogInformation("-*CONNECT TO MARKET*-");
        cryptoProcessing.ForEach(async x => await x.ConnectToMarketAsync());

        _appRobo = HostApp.Host.Services.GetService<IAppRobo>() ??
                   throw new NullReferenceException("IAppRobo services is not registered!");

        _logger.LogInformation("-*START ROBO STRATEGY*-");
        var delay = 3000;
        while (!stoppingToken.IsCancellationRequested)
            try
            {
                _counter++;
                foreach (var marketCoreBroker in cryptoProcessing)
                {
                    await marketCoreBroker.RunAsync();
                    await Task.Delay(_appRobo.Config.WaitingBetweenStrategyInMiliSeconds, stoppingToken);
                }

                if (_counter == Int32.MaxValue) _counter = 0;
                if (_counter % 10 == 0) await _appRobo.RoboConfig.SaveDataAsync(stoppingToken);
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