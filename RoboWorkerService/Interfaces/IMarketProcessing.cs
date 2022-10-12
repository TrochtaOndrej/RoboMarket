using RoboWorkerService.Market.Enum;

namespace RoboWorkerService.Interfaces;

public interface IMarketProcessing
{
    Task InitAsync(CryptoCurrency walletCryptoCurrency);
    Task RunAsync();
}