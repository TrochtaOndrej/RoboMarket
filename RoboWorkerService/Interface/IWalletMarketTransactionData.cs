using ExchangeSharp;
using RoboWorkerService.Market.Enum;

namespace RoboWorkerService.Interface;

public interface IWalletMarketTransactionData<T> where T : ICryptoCurrency
{
    void AddTransaction(ExchangeOrderResult exchangeOrderResult);
}