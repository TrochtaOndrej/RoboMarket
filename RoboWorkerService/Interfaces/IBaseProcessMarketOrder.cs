using ExchangeSharp;
using RoboWorkerService.Interface;
using RoboWorkerService.Market.Enum;
using RoboWorkerService.Market.Model;

namespace RoboWorkerService.Interfaces;

public interface IProcessAllMarketOrder<T> : IBaseProcessMarketOrder<T>  where T : ICryptoCurrency
{
    void Init();
    void SaveWallet();

    MarketProcessBuyOrSell? RunProcessing(ExchangeTicker ticker);
}

public interface IBaseProcessMarketOrder<T> : IMarketCrypto where T : ICryptoCurrency
{
    IWallet<T> Wallet { get; }
}