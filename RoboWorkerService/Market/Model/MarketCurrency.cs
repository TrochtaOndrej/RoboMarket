using Breaker.Helpers.Extensions;
using RoboWorkerService.Market.Enum;

namespace RoboWorkerService.Market.Model;

public record MarketCurrency
{
    public CryptoCurrency CryptoCurrency { get; set; }

    public MarketCurrency(CryptoCurrency cryptoCurrencyType)
    {
        CryptoCurrency = cryptoCurrencyType;
        MarketSymbol = cryptoCurrencyType.ToString();
    }

    /// <summary> Get BTC-EUR as string </summary>
    public string MarketSymbol { get; }

   
}