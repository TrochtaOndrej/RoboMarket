using Newtonsoft.Json;

namespace RoboWorkerService.Interfaces;

public interface IWallet<T> : IWallet
{ }

public interface IWallet : IMarketCurrency
{
    Dictionary<string, IWallet> ProcessingWallet { get; set; }
    /// <summary>BTC na ucte </summary>
    decimal CryptoAccountValue { get; set; }

    /// <summary> EUR na ucte </summary>
    decimal EurAccountValue { get; set; }

    /// <summary> CZK na ucte </summary>
    [JsonIgnore]
    decimal CzkAccountValue { get; }

    /// <summary>The ask is the price to buy at </summary>
    decimal CryptoPositionTransaction { get; set; }

    public void SetWallet(string key, IWallet w);
    public IWallet? GetWallet(string key);

    /// <summary>prepocita globa Wallet souctem vsech Broker wallet. Broker wallet je strategie obchodovani</summary>
    void SumAllWalletAndInsertIntoGlobalWallet();
}