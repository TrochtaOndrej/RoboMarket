using ExchangeSharp;
using RoboWorkerService.Market.Enum;
using RoboWorkerService.Market.Model;

namespace RoboWorkerService.Interface;

public interface ICupProcessMarketValue
{
    /// <summary> Zakolik se nakupuje</summary>
  //  decimal GetWallet_EurToCryptoBuy();

    /// <summary> Zakolik se prodava</summary>
//    decimal GetWallet_EurToCryptoSell();

    CryptoCurrency CryptoCurrency { get; set; }

    /// <summary> Value BTC, ETH </summary>
    decimal CryptoPriceBuy { get; set; }

    decimal CryptoPriceSell { get; set; }
    void SetActualValue(ExchangeTicker ticker);
    public MarketProcessBuyOrSell? CreateBuyOrder( decimal defineProfitEur);
    public MarketProcessBuyOrSell? CreateSellOrder( decimal defineProfitEur);
    public MarketProcessType GetActualMArketProcess();
    public decimal GetWallet_EurToCrypto();
}