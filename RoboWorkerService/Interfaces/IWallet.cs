using Newtonsoft.Json;

namespace RoboWorkerService.Interfaces;

public interface IWallet<T, X> : IWallet<T>
{
    IWallet? IWallet.GetWallet(string type)
    {
        return GetWallet(type);
    }
    IWallet? IWallet.GetWallet(Type type)
    {
        return GetWallet(type);
    }
}

public interface IWallet<T> : IWallet
{ }

public interface IWallet : IMarketCurrency
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

    public void SetWallet(string key, IWallet w);
    public IWallet? GetWallet(string key);

    public IWallet? GetWallet(Type key);
}