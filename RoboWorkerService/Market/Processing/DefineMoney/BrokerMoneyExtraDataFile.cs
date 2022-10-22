using Helper.Serialization;
using RoboWorkerService.Config;
using RoboWorkerService.Interfaces;
using RoboWorkerService.Market.Enum;

namespace RoboWorkerService.Market.Processing.DefineMoney;

public record BrokerMoneyExtraDataFile<T> : IBrokerMoneyExtraDataFile<T> where T : ICryptoCurrency
{
    private readonly IJsonConvertor _jsonConvertor;
    private readonly IConfig _config;
    private readonly string _filename;
    private BrokerMoneyProcessExtraData _extraData = new BrokerMoneyProcessExtraData();

    public BrokerMoneyExtraDataFile(IJsonConvertor jsonConvertor, IConfig config, T cryptoCurrency)
    {
        _jsonConvertor = jsonConvertor;
        _config = config;
        _filename = config.ConfigPath + cryptoCurrency.Crypto + "_" + nameof(SharpProcessingMarket<T>) + ".json";
    }

    public async Task<BrokerMoneyProcessExtraData> LoadFromFileAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_filename))
        {
            await SaveAsync(new BrokerMoneyProcessExtraData(), cancellationToken);
        }

        return await _jsonConvertor.FileToInstanceAsync<BrokerMoneyProcessExtraData>(_filename, cancellationToken);
    }

    public Task SaveAsync(BrokerMoneyProcessExtraData brokerMoneyProcessExtraData, CancellationToken cancellationToken = default)
    {
        return _jsonConvertor.ToFileJsonAsync(_filename, brokerMoneyProcessExtraData, cancellationToken);
    }
}