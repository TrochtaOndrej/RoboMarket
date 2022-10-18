using ExchangeSharp;
using RoboWorkerService.Market.Enum;
using RoboWorkerService.Market.Model;

namespace RoboWorkerService.Interfaces;

public interface ICupProcessingMarket<W> : IProcessAllMarketOrder<W> where W : ICryptoCurrency
{
    MarketProcessBuyOrSell? RunProcessing(ExchangeTicker ticker);
}