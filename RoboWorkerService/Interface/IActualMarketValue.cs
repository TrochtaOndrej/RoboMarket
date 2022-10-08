using ExchangeSharp;
using RoboWorkerService.Market.Enum;
using RoboWorkerService.Market.Model;

namespace RoboWorkerService.Interface;

public interface IActualMarketValue
{
    /// <summary> Zakolik se nakupuje</summary>
    decimal GetWallet_EurToCryptoBuy(Wallet w);

    /// <summary> Zakolik se prodava</summary>
    decimal GetWallet_EurToCryptoSell(Wallet w);

    MarketCurrencyType MarketCurrency { get; set; }

    /// <summary> Value BTC, ETH </summary>
    decimal CryptoPriceBuy { get; set; }

    decimal CryptoPriceSell { get; set; }
    void SetActualValue(ExchangeTicker ticker);
    public MarketProcessBuyOrSell? CreateBuyOrder(Wallet actualWallet, decimal defineProfitEur);
    public MarketProcessBuyOrSell? CreateSellOrder(Wallet actualWallet, decimal defineProfitEur);
    public MarketProcessType GetActualMArketProcess(Wallet actualWallet);
    public decimal GetWallet_EurToCrypto(Wallet w);
}