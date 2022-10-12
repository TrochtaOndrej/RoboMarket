using ExchangeSharp;
using RoboWorkerService.Market.Enum;
using RoboWorkerService.Market.Model;

namespace RoboWorkerService.Interfaces;

public interface IProcessingMarketValue<W> : IBaseProcessMarketOrder<W> where W : ICryptoCurrency
{
    public MarketProcessBuyOrSell? RunProcessing(ExchangeTicker ticker);
}


//public interface IProcessingMarketValue: IBaseProcessMarketOrder
//{
//    MarketProcessBuyOrSell? RunProcessing(ExchangeTicker ticker);
//}