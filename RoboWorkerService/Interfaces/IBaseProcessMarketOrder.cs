using ExchangeSharp;
using RoboWorkerService.Interface;
using RoboWorkerService.Market.Enum;
using RoboWorkerService.Market.Model;

namespace RoboWorkerService.Interfaces;

public interface IProcessAllMarketOrder<T> : IBaseProcessMarketOrder<T> where T : ICryptoCurrency
{
    void Init();
    Task SaveWalletAsync();
    void CalculateGlobalWallet();
}

public interface IBaseProcessMarketOrder<T> : IMarketCrypto where T : ICryptoCurrency
{
    IWallet<T> GlobalWallet { get; }
    void SetBrokerWallet(IWallet brokerWallet);
}