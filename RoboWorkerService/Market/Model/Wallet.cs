using Newtonsoft.Json;
using RoboWorkerService.Interface;
using RoboWorkerService.Interfaces;
using RoboWorkerService.Market.Enum;

namespace RoboWorkerService.Market.Model;

public record Wallet<T> : Wallet, IWallet<T>, IWallet where T : ICryptoCurrency
{
    public Wallet(T cryptoCurrency) : base(cryptoCurrency)
    { }
}

public record Wallet : MarketCurrency, IWallet
{
    public Dictionary<string, IWallet> CryptoBrokerWallet { get; set; } = new Dictionary<string, IWallet>();

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
        if (CryptoBrokerWallet.ContainsKey(key))

            CryptoBrokerWallet[key] = w;
        else
            CryptoBrokerWallet.Add(key, w);
    }

    public IWallet? GetWallet(string key)
    {
        CryptoBrokerWallet.TryGetValue(key, out var wallet);
        return wallet;
    }

    /// <summary> vypocita global Wallet hodnoty z pod uctu </summary>
    /// <param name="key"></param>
    /// <param name="w"></param>
    /// <returns></returns>
    public void SumAllWalletAndInsertIntoGlobalWallet()
    {
        lock (this)
        {
            var cryptoValue = 0m;
            var eur = 0m;
            foreach (var wallet in CryptoBrokerWallet)
            {
                cryptoValue += wallet.Value.CryptoAccountValue;
                eur += wallet.Value.EurAccountValue;
            }

            EurAccountValue = eur;
            CryptoAccountValue = cryptoValue;
        }
    }
}