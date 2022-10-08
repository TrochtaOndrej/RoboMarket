using System.ComponentModel;
using ExchangeSharp;
using Newtonsoft.Json;
using RoboWorkerService.Market.Enum;

namespace RoboWorkerService.Market.Model;

public record Wallet : MarketCurrency
{
    /// <summary>BTC na ucte </summary>
    public decimal CryptoAccountValue { get; set; }

    /// <summary> EUR na ucte </summary>
    public decimal EurAccountValue { get; set; }

    /// <summary> CZK na ucte </summary>
    public decimal CzkAccountValue => EurAccountValue * 25;

    public override string ToString()
    {
        return $"WALLET {NameCurrency} {CryptoAccountValue} |EUR {EurAccountValue}| CZK {CzkAccountValue}";
    }

    public Wallet(MarketCurrencyType currencyType) : base(currencyType)
    { }

    public static void SaveWalletToJsonFile(Wallet wallet)
    {
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