using RoboWorkerService.Interfaces;
using RoboWorkerService.Market.Enum;
using RoboWorkerService.Market.Processing;
using RoboWorkerService.Robo;

namespace RoboWorkerService.Market;

/// <summary> Jedna se o Cup metodu zpracovani BUY or SELL</summary>
/// <typeparam name="W"></typeparam>
public class MarketCoreCupBroker<W> : MarketCore<W>, IMarketCoreCupBroker<W> where W : ICryptoCurrency
{
    private readonly ICupProcessingMarket<W> _pm;
    private readonly ILogger<MarketCoreCupBroker<W>> _logger;
    readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

    public MarketCoreCupBroker(
        ICupProcessingMarket<W> pm,
        ICoinMateRobo<W> cmr,
        ILogger<MarketCoreCupBroker<W>> logger) : base(cmr, logger)
    {
        _pm = pm;
        _logger = logger;
    }

    public async Task ConnectToMarketAsync()
    {
        _pm.Init();
        await _cmr.InitRoboAsync((W)_pm.Wallet.CryptoCurrency);
    }

    public Task RunAsync()
    {
        return CheckMarket();
    }

    private async Task CheckMarket()
    {
        await _semaphoreSlim.WaitAsync();
        try
        {
            // nacti novy order z Burzy
            var ticker = await IsTheSameTickerWithLastTickerAsync();
            if (ticker is null) return;

            //vypocti profit 
            var buyOrSell = _pm.RunProcessing(ticker);
            if (buyOrSell == null) return; // zadny profit 

            _logger.LogDebug(ObjectDumper.Dump(buyOrSell));

            // vytvor platbu (orderPlate)
            var resultOrder = await _cmr.PlaceOrderAsync(buyOrSell);
            _logger.LogDebug("Actual transaction {@transaction}", resultOrder);
            // _pm.AddTransaction(resultOrder);
            CalculateActualTransactionIntoWallet(_pm.Wallet, resultOrder); // snizi a zvysi hodnotu
            _pm.SaveWallet();
            _logger.LogDebug(ObjectDumper.Dump(resultOrder));

            Console.WriteLine();
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

}