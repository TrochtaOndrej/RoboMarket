using RoboWorkerService.Market.Processing;

namespace RoboWorkerService.Interfaces;

public interface IBrokerMoneyProcessExtraDataService<W>
{
    void AddTransaction(TransactionData transactionData);
    Task SaveData();
}