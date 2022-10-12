using ExchangeSharp;
using RoboWorkerService.Market.Enum;
using RoboWorkerService.Market.Model;

namespace RoboWorkerService.Robo;

public interface ICoinMateRobo<T> where T : ICryptoCurrency
{
    public Task InitRoboAsync(T symbol);
    public Task<IEnumerable<ExchangeOrderResult>> GetValueAsync();
    Task<string> ExchangeRateBtcAsync();

    /// <summary> Aktualni hodnota na burze  </summary>
    public Task<ExchangeTicker> GetTickerAsync();

    /// <summary>Nakup nebo prodej na Marketu</summary>
    public Task<ExchangeOrderResult> PlaceOrderAsync(MarketProcessBuyOrSell marketProcessBuyOrSell);
}