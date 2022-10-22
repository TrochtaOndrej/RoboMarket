using RoboWorkerService.Market.Model;
using RoboWorkerService.Market.Processing;
using RoboWorkerService.Market.Processing.DefineMoney;

namespace RoboWorkerService.Interfaces;

public interface IBrokerMoneyProcessExtraDataService<W>
{
    void AddTransaction(TransactionData transactionData);
    Task SaveDataAsync();
    IEnumerable<TransactionData> GetOpenOrderTransaction(Func<TransactionData, bool>? where = null);
    bool RemoveTransaction(TransactionData transactionData);
    /// <summary> Data pro strategii, </summary>
    MoneyProcessDataBuy MoneyProcessDataBuy { get; }
    MoneyProcessDataSell MoneyProcessDataSell { get; }
}