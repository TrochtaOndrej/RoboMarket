using ExchangeSharp;
using Helper.Interface;
using Helper.Serialization;
using NLog.LayoutRenderers.Wrappers;
using RoboWorkerService.Config;
using RoboWorkerService.Interfaces;
using RoboWorkerService.Market.Enum;
using RoboWorkerService.Market.Model;

namespace RoboWorkerService.Market.Processing;

public class TransactionsDataDriver<T> : ITransactionDataDriver<T> where T : ICryptoCurrency
{
    private readonly IJsonConvertor _json;
    private readonly IWallet _wallet;
    public List<TransactionData> Trasactions = new List<TransactionData>();
    private readonly string _fileName;

    public TransactionsDataDriver(IJsonConvertor json, IConfig config, IWallet<T> wallet)
    {
        _json = json;
        _wallet = wallet;
        _fileName = config.ConfigPath + _wallet.MarketSymbol + "_Transaction.json";
        if (!File.Exists(_fileName))
            SaveAsync().Wait(1000);
        else
            Load().Wait(10000);
    }

    public TransactionData Add(ExchangeOrderRequest request, ExchangeOrderResult result, IWallet wallet,
        MarketProcessBuyOrSell buyOrSell, string strategyName)
    {
        var data = new TransactionData
        {
            Wallet = wallet,
            OrderRequest = request,
            OrderResult = result,
            BuyOrSell = buyOrSell,
            StrategyName = strategyName
        };
        Add(data);
        return data;
    }

    public void Add(TransactionData transaction)
    {
        Trasactions.Add(transaction);
    }

    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        await _json.ToFileJsonAsync(_fileName, Trasactions, cancellationToken);
    }

    public async Task Load(CancellationToken cancellationToken = default)
    {
        if (File.Exists(_fileName))
            Trasactions = await _json.FileToInstanceAsync<List<TransactionData>>(_fileName, cancellationToken);
        else
        {
            Trasactions = new List<TransactionData>();
            await SaveAsync(cancellationToken);
        }
    }

    public IEnumerable<TransactionData> GetTransaction(Func<TransactionData, bool> fnc)
    {
        return Trasactions.Where(fnc);
    }
}