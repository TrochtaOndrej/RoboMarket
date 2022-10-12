using RoboWorkerService.Market.Enum;

namespace RoboWorkerService.Interfaces;

public interface IMarketCrypto : ICryptoCurrency
{
    public CryptoCurrency CryptoCurrency { get; set; }

    /// <summary> position in market pro nakup (napr. 19957)</summary>
    public decimal CryptoPriceBuy { get; set; }

    /// <summary> position in market pro prodej (napr. 19927)</summary>
    public decimal CryptoPriceSell { get; set; }
}