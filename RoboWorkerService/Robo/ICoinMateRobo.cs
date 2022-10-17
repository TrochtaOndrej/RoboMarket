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

    /// <summary>Vytvor platebni prikaz pro Market</summary>
    ExchangeOrderRequest CreateExchangeOrderRequest(MarketProcessBuyOrSell marketProcessBuyOrSell);

    /// <summary>  Buy or Sell on market </summary>
    Task<ExchangeOrderResult> PlaceOrderAsync(ExchangeOrderRequest orderRequest);

    Task<IEnumerable<ExchangeOrderResult>> GetOpenOrderDetailsAsync();
}