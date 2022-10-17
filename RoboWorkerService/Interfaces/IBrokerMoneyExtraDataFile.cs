using RoboWorkerService.Market.Processing.DefineMoney;

namespace RoboWorkerService.Interfaces;

public interface IBrokerMoneyExtraDataFile
{
    BrokerMoneyProcessExtraData LoadFromFile();
    Task Save(BrokerMoneyProcessExtraData brokerMoneyProcessExtraData);
}