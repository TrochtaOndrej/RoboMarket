using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RoboWorkerService.Market.Enum;

public enum MarketCurrencyType
{
    [Description("BTC-EUR")]
    BTC_EUR,
    [Description("ETH-EUR")]
    ETH_EUR
}