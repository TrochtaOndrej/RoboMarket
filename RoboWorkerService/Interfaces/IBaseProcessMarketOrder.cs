using RoboWorkerService.Interface;
using RoboWorkerService.Market.Enum;

namespace RoboWorkerService.Interfaces;

public interface IBaseProcessMarketOrder<T> : IMarketCrypto where T : ICryptoCurrency
{
    IWallet<T> Wallet { get; }
}