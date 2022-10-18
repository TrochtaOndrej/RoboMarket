using Helper.Serialization;
using RoboWorkerService.Config;
using RoboWorkerService.Interfaces;
using RoboWorkerService.Market.Enum;

namespace RoboWorkerService.Market.Processing.DefineMoney;

public record BrokerMoneyExtraDataFile<T> : IBrokerMoneyExtraDataFile where T : CryptoCurrency
{
    private readonly IJsonConvertor _jsonConvertor;
    private readonly IConfig _config;
    private readonly string _filename;
    private BrokerMoneyProcessExtraData _extraData = new BrokerMoneyProcessExtraData();

    public BrokerMoneyExtraDataFile(IJsonConvertor jsonConvertor, IConfig config)
    {
        _jsonConvertor = jsonConvertor;
        _config = config;
        _filename = config.ConfigPath + nameof(DefinedMoneyProcessMarket<T>) + "_" + typeof(T).Name + ".json";
    }

    public BrokerMoneyProcessExtraData LoadFromFile()
    {
        return _jsonConvertor.ToInstance<BrokerMoneyProcessExtraData>(_filename);
    }

    public Task SaveAsync(BrokerMoneyProcessExtraData brokerMoneyProcessExtraData)
    {
        return _jsonConvertor.ToFileJsonAsync(_filename, brokerMoneyProcessExtraData);
    }
}