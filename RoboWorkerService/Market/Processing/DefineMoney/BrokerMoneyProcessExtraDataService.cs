using RoboWorkerService.Interfaces;
using RoboWorkerService.Market.Enum;

namespace RoboWorkerService.Market.Processing.DefineMoney;

public class BrokerMoneyProcessExtraDataService<W> : IBrokerMoneyProcessExtraDataService<W> where W : CryptoCurrency
{
    private readonly IBrokerMoneyExtraDataFile _moneyExtraDataFile;
    private BrokerMoneyProcessExtraData _data;

    public BrokerMoneyProcessExtraDataService(IBrokerMoneyExtraDataFile moneyExtraDataFile)
    {
        _moneyExtraDataFile = moneyExtraDataFile;
        _data = moneyExtraDataFile.LoadFromFile();
    }

    public void AddTransaction(TransactionData transactionData)
    {
        _data.TransactionData.Add(transactionData);
    }

    public Task SaveData()
    {
        return _moneyExtraDataFile.Save(_data);
    }
}