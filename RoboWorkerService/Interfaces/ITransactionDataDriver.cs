using ExchangeSharp;
using RoboWorkerService.Market.Enum;
using RoboWorkerService.Market.Model;

namespace RoboWorkerService.Interfaces;

public interface ITransactionDataDriver<T> where T : ICryptoCurrency
{
    void Add(TransactionData transaction);
    Task SaveAsync(CancellationToken cancellationToken = default);
    Task Load(CancellationToken cancellationToken = default);

    TransactionData Add(ExchangeOrderRequest request, ExchangeOrderResult result, IWallet wallet,
        MarketProcessBuyOrSell buyOrSell, string strategyName);

    IEnumerable<TransactionData> GetTransaction(Func<TransactionData, bool> fnc);
}