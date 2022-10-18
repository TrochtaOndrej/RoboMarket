using RoboWorkerService.Market.Enum;

namespace RoboWorkerService.Market.Model;

public record MarketProcessBuyOrSell : MarketCurrency
{
    /// <summary> Ukon - Nakup nebo prodej </summary>
    public MarketProcessType ProcessType { get; set; }

    /// <summary>  BTC, ETH, hodnota za kterou nakoupit nebo prodat </summary>
    public decimal CryptoValue { get; set; }

    public decimal EurValue { get; set; }

    public override string ToString()
    {
        return $"{ProcessType} - {CryptoValue}{CryptoCurrency}";
    }

    /// <summary> Aktualni hodnota marketu (kde koupit nebo prodat) napr: 428 454</summary>
    public decimal Price { get; set; }

    public decimal Fees { get; set; }
    public bool IsPostOnly { get; set; } 

    public MarketProcessBuyOrSell(CryptoCurrency cryptoCurrency) : base(cryptoCurrency)
    {
    }
}