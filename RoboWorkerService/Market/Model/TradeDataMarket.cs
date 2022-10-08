using RoboWorkerService.Market.Enum;

namespace RoboWorkerService.Market.Model;

public record TradeDataMarket : MarketCurrency
{
    public TradeDataMarket(MarketCurrencyType nameCurrency) : base(nameCurrency)
    { }

    public List<TradeData> TradeData = new List<TradeData>();
}