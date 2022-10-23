using System.Security;
using ExchangeSharp;
using RoboWorkerService.Config;
using RoboWorkerService.Interfaces;
using RoboWorkerService.Market.Enum;
using RoboWorkerService.Market.Model;

namespace RoboWorkerService.Robo;

public class CoinMateRobo<T> : ICoinMateRobo<T> where T : ICryptoCurrency
{
    public static CoinMateRobo<T> CoinRobo;
    private IExchangeAPI _iexApi = default!;
    private string _marketSymbol;

    public async Task InitRoboAsync(T symbol, IAppRobo appRobo)
    {
        var configFile = appRobo.Config.RootPath + @"\coinbase.bin";
        if (!File.Exists(configFile)) throw new FileNotFoundException("Config file not found. Path: " + appRobo);

        _iexApi = await ExchangeAPI.GetExchangeAPIAsync(typeof(ExchangeCoinbaseAPI));
        _iexApi.LoadAPIKeys(configFile);
        _iexApi.Passphrase = new SecureString();
        _iexApi.Cache = new MemoryCache(299);
        _iexApi.MethodCachePolicy["GetTickerAsync"] = TimeSpan.FromMilliseconds(300);

        ExchangeAPI.UseDefaultMethodCachePolicy = false;

        _marketSymbol = symbol.Crypto;

        foreach (char c in "trochin") _iexApi.Passphrase.AppendChar(c);
    }

    public Task<IEnumerable<ExchangeOrderResult>> GetValueAsync()
    {
        return _iexApi.GetOpenOrderDetailsAsync(_marketSymbol);
    }

    public Task<string> ExchangeRateBtcAsync()
    {
        return _iexApi.GlobalMarketSymbolToExchangeMarketSymbolAsync(_marketSymbol);
    }

    /// <summary> Aktualni hodnota na burze </summary>
    public Task<ExchangeTicker> GetTickerAsync()
    {
        return _iexApi.GetTickerAsync(_marketSymbol);
    }

    public Task<IEnumerable<ExchangeOrderResult>> GetOpenOrderDetailsAsync()
    {
        return _iexApi.GetCompletedOrderDetailsAsync(_marketSymbol,
            DateTime.Now.Date); //TODO OT: doresit datum od kdy si vyhledat uzavrene ordery
        return _iexApi.GetOpenOrderDetailsAsync(_marketSymbol);
    }

    public ExchangeOrderRequest CreateExchangeOrderRequest(MarketProcessBuyOrSell marketProcessBuyOrSell)
    {
        var request = new ExchangeOrderRequest
        {
            OrderType = OrderType.Limit,
            IsBuy = marketProcessBuyOrSell.ProcessType == MarketProcessType.Buy,
            MarketSymbol = marketProcessBuyOrSell.MarketSymbol,
            Price = marketProcessBuyOrSell.Price,
            // IsPostOnly = marketProcessBuyOrSell.IsPostOnly
        };

        if (marketProcessBuyOrSell.ProcessType == MarketProcessType.Buy)
        {
            request.Amount = marketProcessBuyOrSell.CryptoValue;
        }

        if (marketProcessBuyOrSell.ProcessType == MarketProcessType.Sell)
        {
            request.Amount = marketProcessBuyOrSell.CryptoValue;
        }

        return request;
    }

    /// <summary>  Buy or Sell on market </summary>
    public Task<ExchangeOrderResult> PlaceOrderAsync(ExchangeOrderRequest orderRequest)
    {
        return _iexApi.PlaceOrderAsync(orderRequest);
    }
}