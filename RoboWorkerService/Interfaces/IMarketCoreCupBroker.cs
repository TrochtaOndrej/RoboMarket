﻿using RoboWorkerService.Market.Enum;

namespace RoboWorkerService.Interfaces;

public interface IMarketCoreDefinedMoneyBroker<W> : IMarketCoreBroker where W : ICryptoCurrency
{
}

public interface IMarketCoreCupBroker<W> : IMarketCoreBroker where W : ICryptoCurrency
{
}

public interface IMarketCoreBroker
{
    Task RunAsync();
    Task ConnectToMarketAsync();

   // IWallet BrokerWallet { get; }
}