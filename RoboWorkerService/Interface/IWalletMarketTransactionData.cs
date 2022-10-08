using ExchangeSharp;

namespace RoboWorkerService.Interface;

public interface IWalletMarketTransactionData
{
    void AddTransaction(ExchangeOrderResult exchangeOrderResult);
}