using Newtonsoft.Json;
using RoboWorkerService.Market.Enum;

namespace RoboWorkerService.Market.Model;

public interface IMarketCurrency
{
    ICryptoCurrency CryptoCurrency { get; set; }

    /// <summary> Get BTC-EUR as string </summary>
    string MarketSymbol { get; }
}

public record MarketCurrency : IMarketCurrency
{
    [JsonIgnore]
    public ICryptoCurrency CryptoCurrency { get; set; }

    public MarketCurrency()
    {
        
    }

    public MarketCurrency(ICryptoCurrency cryptoCurrencyType)
    {
        CryptoCurrency = cryptoCurrencyType;
        MarketSymbol = cryptoCurrencyType?.Crypto.ToString();
    }

    /// <summary> Get BTC-EUR as string </summary>
    public string MarketSymbol { get; set; }
}