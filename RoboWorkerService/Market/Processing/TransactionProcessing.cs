using ExchangeSharp;
using Helper.Serialization;
using NLog.LayoutRenderers.Wrappers;
using RoboWorkerService.Config;
using RoboWorkerService.Interfaces;
using RoboWorkerService.Market.Enum;
using RoboWorkerService.Market.Model;

namespace RoboWorkerService.Market.Processing;

public class TransactionProcessing<T> : ITransactionProcessing<T> where T : ICryptoCurrency
{
    private readonly IJsonConvertor _json;
    private readonly IWallet _wallet;
    public List<TransactionData> Trasactions = new List<TransactionData>();
    private readonly string _fileName;

    public TransactionProcessing(IJsonConvertor json, IConfig config, IWallet<T> wallet)
    {
        _json = json;
        _wallet = wallet;
        _fileName = config.ConfigPath + _wallet.MarketSymbol + "_Transaction.json";
        if (!File.Exists(_fileName))
            SaveAsync().Wait(1000);
        else
            Load().Wait(10000);
    }

    public void Add<X>(ExchangeOrderRequest request, ExchangeOrderResult result, IWallet wallet,
        MarketProcessBuyOrSell buyOrSell, X typeTransaction) where X : class
    {
        Add(new TransactionData
        {
            Wallet = wallet,
            OrderRequest = request,
            OrderResult = result,
            BuyOrSell = buyOrSell,
            ProcessingType = typeTransaction.ToString()
        });
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
        Trasactions = await _json.FileToInstanceAsync<List<TransactionData>>(_fileName, cancellationToken);
    }
}

public interface ITransactionProcessing<T> where T : ICryptoCurrency
{
    void Add(TransactionData transaction);
    Task SaveAsync(CancellationToken cancellationToken = default);
    Task Load(CancellationToken cancellationToken = default);
    void Add<X>(ExchangeOrderRequest request, ExchangeOrderResult result, IWallet wallet, MarketProcessBuyOrSell buyOrSell, X typeProcessing) where X : class;
}

public record TransactionData
{
    public ExchangeOrderRequest OrderRequest { get; set; }
    public ExchangeOrderResult OrderResult { get; set; }
    public IWallet Wallet { get; set; }
    public MarketProcessBuyOrSell BuyOrSell { get; set; }

    public string ProcessingType { get; set; }
}
