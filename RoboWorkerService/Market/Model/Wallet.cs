using Newtonsoft.Json;
using RoboWorkerService.Interface;
using RoboWorkerService.Interfaces;
using RoboWorkerService.Market.Enum;

namespace RoboWorkerService.Market.Model;

public record Wallet<T, X> : Wallet<T>, IWallet<T, X> where T : ICryptoCurrency
{
    public Wallet(T cryptoCurrency) : base(cryptoCurrency)
    {
    }
}

public record Wallet<T> : Wallet, IWallet<T>, IWallet where T : ICryptoCurrency
{
    public Wallet(T cryptoCurrency) : base(cryptoCurrency)
    { }
}

public record Wallet : MarketCurrency, IWallet
{
    protected Dictionary<string, IWallet> _processingWallet = new Dictionary<string, IWallet>();

    /// <summary>BTC na ucte </summary>
    public decimal CryptoAccountValue { get; set; }

    /// <summary> EUR na ucte </summary>
    public decimal EurAccountValue { get; set; }

    /// <summary> CZK na ucte </summary>
    [JsonIgnore]
    public decimal CzkAccountValue => EurAccountValue * 25;

    /// <summary>The ask is the price to buy at </summary>
    public decimal CryptoPositionTransaction { get; set; }

    public Wallet(ICryptoCurrency cryptoCurrency) : base(cryptoCurrency)
    {
    }

    public override string ToString()
    {
        return $"WALLET {CryptoCurrency} {CryptoAccountValue} |EUR {EurAccountValue}| CZK {CzkAccountValue}";
    }

    public void SetWallet(string key, IWallet w)
    {
        if (_processingWallet.ContainsKey(key))

            _processingWallet[key] = w;
        else
            _processingWallet.Add(key, w);
    }

    public IWallet? GetWallet(string key)
    {
        _processingWallet.TryGetValue(key, out var wallet);
        return wallet;
    }

    public IWallet? GetWallet(Type key)
    {
        throw new NotImplementedException();
    }
}