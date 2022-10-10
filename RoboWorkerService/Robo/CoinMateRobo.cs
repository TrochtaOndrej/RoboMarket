using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using Breaker.Helpers.Extensions;
using ExchangeSharp;
using RoboWorkerService.Market.Enum;
using RoboWorkerService.Market.Model;

namespace RoboWorkerService.Robo;

public interface ICoinMateRobo
{
    public Task InitRoboAsync(CryptoCurrency symbol);
    public Task<IEnumerable<ExchangeOrderResult>> GetValueAsync();
    Task<string> ExchangeRateBtcAsync();
    /// <summary> Aktualni hodnota na burze  </summary>
    public Task<ExchangeTicker> GetTickerAsync();

    /// <summary>Nakup nebo prodej na Marketu</summary>
    public Task<ExchangeOrderResult> PlaceOrderAsync(MarketProcessBuyOrSell marketProcessBuyOrSell);
}

public class CoinMateRobo : ICoinMateRobo
{
    public static CoinMateRobo CoinRobo;
    private IExchangeAPI _iexApi = default!;
    private string _marketSymbol;

    public CoinMateRobo()
    { }
    public async Task InitRoboAsync(CryptoCurrency symbol)
    {
        _iexApi = await ExchangeAPI.GetExchangeAPIAsync(typeof(ExchangeCoinbaseAPI));
        _iexApi.LoadAPIKeys(Environment.CurrentDirectory + @"\coinbase.bin");
        _iexApi.Passphrase = new SecureString();
        _iexApi.Cache = new MemoryCache(299);
        _iexApi.MethodCachePolicy["GetTickerAsync"] = TimeSpan.FromMilliseconds(300);

        ExchangeAPI.UseDefaultMethodCachePolicy = false;

        _marketSymbol = symbol.ToString();

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

    /// <summary>  Buy or Sell on market </summary>
    public Task<ExchangeOrderResult> PlaceOrderAsync(MarketProcessBuyOrSell marketProcessBuyOrSell)
    {
        ExchangeOrderRequest ech = new ExchangeOrderRequest()
        {
            Amount = marketProcessBuyOrSell.CryptoValue,
            OrderType = OrderType.Limit,
            IsBuy = marketProcessBuyOrSell.ProcessType == MarketProcessType.Buy,
            MarketSymbol = marketProcessBuyOrSell.MarketSymbol,
            Price = marketProcessBuyOrSell.Price,
           // IsPostOnly = true
        };

        return _iexApi.PlaceOrderAsync(ech);
    }
}

