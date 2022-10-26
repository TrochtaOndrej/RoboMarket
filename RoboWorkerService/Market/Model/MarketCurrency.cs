using Newtonsoft.Json;
using RoboWorkerService.Config;
using RoboWorkerService.Interfaces;
using RoboWorkerService.Market.Enum;

namespace RoboWorkerService.Market.Model;

public record MarketCurrency : IMarketCurrency
{
    private readonly IConfig _config;
    [JsonIgnore] public ICryptoCurrency CryptoCurrency { get; set; }

    public MarketCurrency()
    {
    }

    public MarketCurrency(ICryptoCurrency cryptoCurrencyType)
    {
        if (cryptoCurrencyType == null) return;
        _config = HostApp.Host.Services.GetService<IConfig>()!;
        CryptoCurrency = cryptoCurrencyType;
        MarketSymbol = cryptoCurrencyType!.Crypto.ToString().Replace('-', _config.CryptoSeparator);
    }

    /// <summary> Get BTC-EUR as string </summary>
    public string MarketSymbol { get; set; }
}