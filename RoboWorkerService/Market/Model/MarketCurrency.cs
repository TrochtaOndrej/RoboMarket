using Breaker.Helpers.Extensions;
using RoboWorkerService.Market.Enum;

namespace RoboWorkerService.Market.Model;

public record MarketCurrency
{
    public MarketCurrencyType NameCurrency { get; }

    public MarketCurrency(MarketCurrencyType currencyType)
    {
        NameCurrency = currencyType;
        MarketSymbol = currencyType.GetEnumDescription();
    }

    /// <summary> Get BTC-EUR as string </summary>
    public string MarketSymbol { get; } 
}