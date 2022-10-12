using RoboWorkerService.Market.Enum;

namespace RoboWorkerService.Interfaces;

public interface IMarketCoreCupBroker<W> where W : ICryptoCurrency
{
    Task RunAsync();
    Task ConnectToMarketAsync();
}