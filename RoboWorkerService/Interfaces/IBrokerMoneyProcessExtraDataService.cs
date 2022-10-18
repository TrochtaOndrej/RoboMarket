using RoboWorkerService.Market.Processing;

namespace RoboWorkerService.Interfaces;

public interface IBrokerMoneyProcessExtraDataService<W>
{
    void AddTransaction(TransactionData transactionData);
    Task SaveDataAsync();
    IEnumerable<TransactionData> GetOpenOrderTransaction(Func<TransactionData, bool>? where = null);
    bool RemoveTransaction(TransactionData transactionData);
}