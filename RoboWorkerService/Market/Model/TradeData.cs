namespace RoboWorkerService.Market.Model;

public record TradeData
{
    /// <summary> BTC, ETH, ... </summary>
    public decimal MarketValue { get; set; }

    public DateTime Date { get; set; }
}