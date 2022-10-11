using ExchangeSharp;
using RoboWorkerService.Market.Enum;
using RoboWorkerService.Market.Model;
using RoboWorkerService.Market.Processing;

namespace RoboWorkerService.Interface;

public interface IProcessingMarketValue<W> : IBaseProcessMarketOrder<W> where W : ICryptoCurrency
{
    public MarketProcessBuyOrSell? RunProcessing(ExchangeTicker ticker);
}


//public interface IProcessingMarketValue: IBaseProcessMarketOrder
//{
//    MarketProcessBuyOrSell? RunProcessing(ExchangeTicker ticker);
//}