using RoboWorkerService.Market.Enum;

namespace RoboWorkerService.Interface;

public interface IMarketProcessing
{
    Task InitAsync(CryptoCurrency walletCryptoCurrency);
    Task RunAsync();
}