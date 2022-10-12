using ExchangeSharp;
using RoboWorkerService.Interface;
using RoboWorkerService.Market.Enum;
using RoboWorkerService.Market.Model;

namespace RoboWorkerService.Market.Processing;

public class MarketTransactionData<T> : IWalletMarketTransactionData<T> where T : ICryptoCurrency
{
    private Transaction _transaction;

    public MarketTransactionData()
    {
        _transaction = new Transaction();
    }

    public void AddTransaction(ExchangeOrderResult exchangeOrderResult)
    {
        _transaction.Add(exchangeOrderResult);
    }
}