using Newtonsoft.Json;

namespace RoboWorkerService.Interfaces;

public interface IWallet<T> : IMarketCurrency
{

    /// <summary>BTC na ucte </summary>
    decimal CryptoAccountValue { get; set; }

    /// <summary> EUR na ucte </summary>
    decimal EurAccountValue { get; set; }

    /// <summary> CZK na ucte </summary>
    [JsonIgnore]
    decimal CzkAccountValue { get; }

    /// <summary>The ask is the price to buy at </summary>
    decimal CryptoPositionTransaction { get; set; }
}