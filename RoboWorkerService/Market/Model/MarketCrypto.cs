using RoboWorkerService.Market.Enum;

namespace RoboWorkerService.Market.Model;

public interface IMarketCrypto : ICryptoCurrency
{
    public CryptoCurrency CryptoCurrency { get; set; }

    /// <summary> position in market pro nakup (napr. 19957)</summary>
    public decimal CryptoPriceBuy { get; set; }

    /// <summary> position in market pro prodej (napr. 19927)</summary>
    public decimal CryptoPriceSell { get; set; }
}

/// <summary> hodnota na burze </summary>
public class MarketCrypto : IMarketCrypto
{
    public CryptoCurrency CryptoCurrency { get; set; }

    /// <summary> position in market pro nakup (napr. 19957)</summary>
    public decimal CryptoPriceBuy { get; set; }

    /// <summary> position in market pro prodej (napr. 19927)</summary>
    public decimal CryptoPriceSell { get; set; }

    public string Crypto
    {
        get { return CryptoCurrency.Crypto; }
        set { CryptoCurrency.Crypto = value; }
    }
}