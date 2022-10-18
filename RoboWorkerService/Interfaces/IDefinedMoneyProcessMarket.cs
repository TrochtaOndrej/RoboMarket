using ExchangeSharp;
using RoboWorkerService.Market.Enum;
using RoboWorkerService.Market.Model;

namespace RoboWorkerService.Interfaces;

public interface IDefinedMoneyProcessMarket<W> : IProcessAllMarketOrder<W> where W : ICryptoCurrency
{
    Task<List<MarketProcessBuyOrSell>> RunProcessingAsync(ExchangeTicker ticker,
        IBrokerMoneyProcessExtraDataService<W> extraDataService,
        List<ExchangeOrderResult> openOrdersActualInMarket);

    MarketProcessBuyOrSell? CreateBuyOrder(decimal defineProfitInPercently, decimal investMoneyEur,
        MarketProcessType marketProcessType);
}