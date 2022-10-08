using ExchangeSharp;
using RoboWorkerService.Interface;
using RoboWorkerService.Market.Model;
using RoboWorkerService.Robo;

namespace RoboWorkerService.Market.Business;

public class WalletMarketTransactionData : IWalletMarketTransactionData
{
    private readonly Wallet _wallet;
    private readonly ICoinMateRobo _coinMateRobo;
    private Transaction _transaction;

    public WalletMarketTransactionData(Wallet wallet, ICoinMateRobo coinMateRobo)
    {
        _wallet = wallet;
        _coinMateRobo = coinMateRobo;
        _transaction = new Transaction();
    }

    public void AddTransaction(ExchangeOrderResult exchangeOrderResult)
    {
        _transaction.Add(exchangeOrderResult);
    }

   
}