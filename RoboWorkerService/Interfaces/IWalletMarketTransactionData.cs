using ExchangeSharp;
using RoboWorkerService.Market.Enum;

namespace RoboWorkerService.Interfaces;

public interface IWalletMarketTransactionData<T> where T : ICryptoCurrency
{
    void AddTransaction(ExchangeOrderResult exchangeOrderResult);
}