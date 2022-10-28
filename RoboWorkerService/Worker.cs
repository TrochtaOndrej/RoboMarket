using Helper;
using RoboWorkerService.Interface;
using RoboWorkerService.Interfaces;
using RoboWorkerService.Market.Processing;
using RoboWorkerService.Telegram;

namespace RoboWorkerService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    public static int Counter = 0;
    private static IAppRobo _appRobo = null!;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        List<Type> typeCryptoOnMarket = new List<Type>()
        {
            typeof(ICryptoBTC),
            typeof(ICryptoETH),
            typeof(ICryptoDOGE),
            typeof(ICryptoALGO),
            typeof(ICryptoALCX)
        };

        List<IMarketCoreBroker> cryptoProcessing = new List<IMarketCoreBroker>();

        foreach (var crypto in typeCryptoOnMarket)
        {
            var marketCoreCrypto =
                (IMarketCoreBroker)HostApp.Host.Services.GetService(
                    typeof(IMarketCoreSharpBroker<>).MakeGenericType(new[] { crypto }))!;
            cryptoProcessing.Add(marketCoreCrypto);
        }

        var botTelegram = HostApp.Host.Services.GetService<ITelegram>() ??
                          throw new NullReferenceException("Tegram service is null");
        botTelegram.TelegramSays += TelegramEvenHandler;

        await botTelegram.SendOkTextAsync("-*CONNECT TO MARKET*-");
        await cryptoProcessing.ForEachAsync(x => x.ConnectToMarketAsync());

        _appRobo = HostApp.Host.Services.GetService<IAppRobo>() ??
                   throw new NullReferenceException("IAppRobo services is not registered!");

        await botTelegram.SendOkTextAsync("-*START ROBO STRATEGY*-");

        while (!stoppingToken.IsCancellationRequested)
            try
            {
                Counter++;
                foreach (var marketCoreBroker in cryptoProcessing)
                {
                    await marketCoreBroker.RunAsync();
                    await Task.Delay(_appRobo.Config.WaitingBetweenStrategyInMiliSeconds, stoppingToken);
                }

                //     await botTelegram.SendOkTextAsync($"Divam se po: {_counter}");
                if (Counter == Int32.MaxValue) Counter = 0;
                if (Counter % 10 == 0) await _appRobo.RoboConfig.SaveDataAsync(stoppingToken);
            }
            catch (Exception e)
            {
                await botTelegram.SendErrorTextAsync(e, "Error, core crash at: { DateTimeOffset.Now}");
                throw;
            }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        return base.StopAsync(cancellationToken);
    }

    public string TelegramEvenHandler(string word)
    {
        word = word.ToUpper();
        switch (word)
        {
            case "/ITERACE":
                return $@"Aktualni smycka: {Counter} ";
            case "/BTCEUR":
                var wallet = _appRobo.GetService<IWallet<ICryptoBTC>>();
                var walletDump = "Wallet: " + Environment.NewLine;
                if (wallet is not null)
                {
                    foreach (var brokerWallet in wallet.CryptoBrokerWallet)
                    {
                        var dumpWallet = brokerWallet.Value.Dump();
                        walletDump += dumpWallet + Environment.NewLine;
                    }

                    return walletDump;
                }

                break;
        }

        return "Nerozeznal jsem prikaz :( ." + Environment.NewLine +
               " Zkus: /Iterace /BTCEUR";
    }
}