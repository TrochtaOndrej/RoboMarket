using ExchangeSharp;
using Newtonsoft.Json;
using RoboWorkerService.Market.Enum;

namespace RoboWorkerService.Market.Model;

public record Wallet<T> : Wallet, IWallet<T> where T : ICryptoCurrency 
{
    public Wallet(T cryptoCurrency) : base(cryptoCurrency)
    { }
}

public interface IWallet<T>: IMarketCurrency
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

public record Wallet : MarketCurrency
{
    /// <summary>BTC na ucte </summary>
    public decimal CryptoAccountValue { get; set; }

    /// <summary> EUR na ucte </summary>
    public decimal EurAccountValue { get; set; }

    /// <summary> CZK na ucte </summary>
    [JsonIgnore]
    public decimal CzkAccountValue => EurAccountValue * 25;

    public override string ToString()
    {
        return $"WALLET {CryptoCurrency} {CryptoAccountValue} |EUR {EurAccountValue}| CZK {CzkAccountValue}";
    }
    /// <summary>The ask is the price to buy at </summary>
    public decimal CryptoPositionTransaction { get; set; }

    public Wallet(ICryptoCurrency cryptoCurrency) : base(cryptoCurrency)
    {

    }

    public static void SaveWalletToJsonFile<T>(IWallet<T> wallet)
    {
        //wallet.MarketSymbol  
        lock (wallet)
            using (StreamWriter file = File.CreateText(@"wallet.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, wallet);
            }
    }
}

public class Transaction
{
    public List<ExchangeOrderResult> Trasactions = new List<ExchangeOrderResult>();

    public void Add(ExchangeOrderResult or)
    {
        Trasactions.Add(or);
    }
}