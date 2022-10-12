using RoboWorkerService.Market.Enum;

namespace RoboWorkerService.Interfaces;

public interface IMarketCurrency
{
    ICryptoCurrency CryptoCurrency { get; set; }

    /// <summary> Get BTC-EUR as string </summary>
    string MarketSymbol { get; }
}