using ExchangeSharp;
using RoboWorkerService.Interfaces;
using RoboWorkerService.Market.Enum;
using RoboWorkerService.Robo;

namespace RoboWorkerService.Market;

public abstract class MarketCore<T> where T : ICryptoCurrency
{
    protected readonly ICoinMateRobo<T> _cmr;
    private readonly ILogger _logger;
    protected abstract IWallet BrokerWallet { get; set; }
    protected abstract string BrokerWalletName { get; }

    public MarketCore(ICoinMateRobo<T> cmr, ILogger logger)
    {
        _cmr = cmr;
        _logger = logger;
    }

    private ExchangeTicker _lastTicker = default!;

    protected async Task<ExchangeTicker?> IsTheSameTickerWithLastTickerAsync()
    {
        var ticker = await _cmr.GetTickerAsync();
        if (ticker.Ask == _lastTicker?.Ask && ticker.Bid == _lastTicker?.Bid)
        {
            Console.Write("-");
            return null;
        }

        //TODO OT LOW: smazat az bude GUI jinde
        Console.WriteLine();

        _logger.LogInformation("Market position BUY: {ask}, SELL {sell}", ticker.Ask, ticker.Bid);

        _lastTicker = ticker;
        return _lastTicker;
    }

    protected async Task GetActualOpenOrders()
    {
        _cmr.GetOpenOrderDetailsAsync();
    }

    protected void CalculateActualTransactionIntoBrokerWallet(ExchangeOrderResult exchange)
    {
        var fees = GetFeesFromOrderInBtc(exchange);
        if (exchange.Price is null) _logger.LogWarning("Price is null after BUY or SELL");
        if (exchange.IsBuy)
        {
            BrokerWallet.CryptoAccountValue += exchange.Amount - fees;
            BrokerWallet.EurAccountValue -= (exchange.Amount - fees) * exchange.Price ?? 0;
        }
        else
        {
            BrokerWallet.CryptoAccountValue -= exchange.Amount - fees;
            BrokerWallet.EurAccountValue += (exchange.Amount - fees) * exchange.Price ?? 0;
        }

        BrokerWallet.CryptoPositionTransaction = exchange.Price ?? 0;


        _logger.LogInformation("Fees {fees} | Transaction {@transaction}", fees, exchange);
        _logger.LogInformation("Actual {brokerWalletName} {wallet}", BrokerWalletName, BrokerWallet.ToString());
    }

    protected decimal GetFeesFromOrderInBtc(ExchangeOrderResult exchange)
    {
        //exchange.Result == ExchangeAPIOrderResult.Filled
        var fees = exchange.Fees.HasValue ? exchange.Fees / 2 : 0;
        if (fees != 0) return fees.Value / exchange.Price ?? 0;

        return 0;
    }
}