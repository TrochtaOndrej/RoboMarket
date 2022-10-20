using ExchangeSharp;
using RoboWorkerService.Interfaces;
using RoboWorkerService.Market.Enum;
using RoboWorkerService.Market.Model;

namespace RoboWorkerService.Market.Processing.DefineMoney;

public class BrokerMoneyProcessExtraDataService<W> : IBrokerMoneyProcessExtraDataService<W> where W : CryptoCurrency
{
    private readonly IBrokerMoneyExtraDataFile _moneyExtraDataFile;
    private readonly ILogger<BrokerMoneyProcessExtraDataService<W>> _logger;
    private BrokerMoneyProcessExtraData _data;
    private bool _isCollectionChanged = false;

    public MoneyProcessDataBuy MoneyProcessDataBuy => _data.ProcessDataBuy;
    public MoneyProcessDataSell MoneyProcessDataSell => _data.ProcessDataSell;


    public BrokerMoneyProcessExtraDataService(IBrokerMoneyExtraDataFile moneyExtraDataFile,
        ILogger<BrokerMoneyProcessExtraDataService<W>> logger)
    {
        _moneyExtraDataFile = moneyExtraDataFile;
        _logger = logger;
        _data = moneyExtraDataFile.LoadFromFile();
    }

    public void AddTransaction(TransactionData transactionData)
    {
        _data.TransactionData.Add(transactionData);
        _isCollectionChanged = true;
    }

    public bool RemoveTransaction(TransactionData transactionData)
    {
        if (_data.TransactionData.Remove(transactionData))
        {
            _isCollectionChanged = true;
            _logger.LogInformation("Removed order transaction OrderId: {O rderId}", transactionData.OrderResult.OrderId);
            return true;
        }

        return false;
    }

    public Task SaveDataAsync()
    {
        if (_isCollectionChanged)
        {
            _isCollectionChanged = false;
            return _moneyExtraDataFile.SaveAsync(_data);
        }

        return Task.CompletedTask;
    }

    public IEnumerable<TransactionData> GetOpenOrderTransaction(Func<TransactionData, bool>? where = null)
    {
        var data = _data.TransactionData.Where(
            x => x.OrderResult.Result.IsCompleted());

        if (where is not null) data = data.Where(where);
        return data;
    }
}