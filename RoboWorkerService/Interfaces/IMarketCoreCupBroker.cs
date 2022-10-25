using RoboWorkerService.Market.Enum;

namespace RoboWorkerService.Interfaces;

public interface IMarketCoreSharpBroker<W> : IMarketCoreBroker where W : ICryptoCurrency
{
}

public interface IMarketCoreCupBroker<W> : IMarketCoreBroker where W : ICryptoCurrency
{
}

public interface IMarketCoreBroker
{
    Task RunAsync();
    Task ConnectToMarketAsync();
    void SetBrokerWallet(IWallet brokerWallet);

}