using ExchangeSharp;
using RoboWorkerService.Market.Enum;
using RoboWorkerService.Market.Model;

namespace RoboWorkerService.Interfaces;

public interface IDefinedMoneyProcessMarket<W> : IProcessAllMarketOrder<W> where W : ICryptoCurrency
{
    Task<List<MarketProcessBuyOrSell>> RunProcessingAsync(ExchangeTicker ticker,
        IBrokerMoneyProcessExtraDataService<W> extraDataService,
        List<ExchangeOrderResult> openOrdersActualInMarket);

    MarketProcessBuyOrSell? CreateBuyOrderEur(decimal defineProfitInPercently, decimal investMoneyEur,
        MarketProcessType marketProcessType);

    public MarketProcessBuyOrSell? CreateBuyOrderCryptoCurrency(decimal defineProfitInPercently, decimal cryptoMoney,
        MarketProcessType marketProcessType);

    List<MarketProcessBuyOrSell> InicializationFirstSharpStrategy(ExchangeTicker firstTicker, IWallet brokerWallet,
        IBrokerMoneyProcessExtraDataService<W> externalData);
}