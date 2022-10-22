using RoboWorkerService.Market.Processing.DefineMoney;

namespace RoboWorkerService.Interfaces;

public interface IBrokerMoneyExtraDataFile<T>
{
    Task SaveAsync(BrokerMoneyProcessExtraData brokerMoneyProcessExtraData,CancellationToken cancellationToken = default);
    Task<BrokerMoneyProcessExtraData> LoadFromFileAsync(CancellationToken cancellationToken = default);
}