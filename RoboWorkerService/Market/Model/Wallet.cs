using Newtonsoft.Json;
using RoboWorkerService.Interface;
using RoboWorkerService.Interfaces;
using RoboWorkerService.Market.Enum;

namespace RoboWorkerService.Market.Model;

public record Wallet<T> : Wallet, IWallet<T> where T : ICryptoCurrency
{
    public Wallet(T cryptoCurrency) : base(cryptoCurrency)
    {
    }
}

public record Wallet : MarketCurrency
{
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
}