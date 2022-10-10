using RoboWorkerService.Market.Enum;

namespace RoboWorkerService.Market.Model;

public record TradeDataMarket : MarketCurrency
{
    public TradeDataMarket(CryptoCurrency nameCryptoCurrency) : base(nameCryptoCurrency)
    { }

    public List<TradeData> TradeData = new List<TradeData>();
}