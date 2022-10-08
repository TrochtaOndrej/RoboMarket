using RoboWorkerService.Market.Enum;

namespace RoboWorkerService.Interface;

public interface IMarketProcessing
{
    Task InitAsync(MarketCurrencyType walletCurrency);
    Task RunAsync();
}