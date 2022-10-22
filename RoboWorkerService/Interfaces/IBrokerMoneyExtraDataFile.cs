using RoboWorkerService.Market.Processing.DefineMoney;

namespace RoboWorkerService.Interfaces;

public interface IBrokerMoneyExtraDataFile<T>
{
    BrokerMoneyProcessExtraData LoadFromFile();
    Task SaveAsync(BrokerMoneyProcessExtraData brokerMoneyProcessExtraData);
}