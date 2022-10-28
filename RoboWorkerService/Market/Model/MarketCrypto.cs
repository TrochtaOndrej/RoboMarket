using RoboWorkerService.Interfaces;
using RoboWorkerService.Market.Enum;

namespace RoboWorkerService.Market.Model;

/// <summary> aktulni hodnota na burze </summary>
public class MarketCrypto : IMarketCrypto
{
    public CryptoCurrency CryptoCurrency { get; set; } = null!;

    /// <summary> position in market pro nakup (napr. 19957)</summary>
    public decimal CryptoPriceBuy { get; set; }

    /// <summary> position in market pro prodej (napr. 19927)</summary>
    public decimal CryptoPriceSell { get; set; }

    public string Crypto
    {
        get { return CryptoCurrency.Crypto; }
        set { CryptoCurrency.Crypto = value; }
    }

    public int CountDecimalNumberInPosition { get; } = 2;
    public int CountDecimalNumberCryptoCurency { get; } = 6;
}