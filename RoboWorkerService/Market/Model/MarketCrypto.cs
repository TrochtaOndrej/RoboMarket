using RoboWorkerService.Market.Enum;

namespace RoboWorkerService.Market.Model;

/// <summary> hodnota na burze </summary>
public record MarketCrypto
{
    public CryptoCurrency CryptoCurrency { get; set; }

    /// <summary> position in market pro nakup (napr. 19957)</summary>
    public decimal CryptoPriceBuy { get; set; }

    /// <summary> position in market pro prodej (napr. 19927)</summary>
    public decimal CryptoPriceSell { get; set; }

}