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
        MarketProcessType marketProcessType, decimal? cryptoPosition = null);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="defineProfitInPercently"></param>
    /// <param name="cryptoMoney"></param>
    /// <param name="marketProcessType"></param>
    /// <param name="cryptoPosition">Pozicce krytpa jinak Price a pokud je null berese aktualni hodnota v Marketu</param>
    /// <returns></returns>
    public MarketProcessBuyOrSell? CreateBuyOrderCryptoCurrency(decimal defineProfitInPercently, decimal cryptoMoney,
        MarketProcessType marketProcessType, decimal? cryptoPosition = null);

    List<MarketProcessBuyOrSell> InicializationFirstSharpStrategy(ExchangeTicker firstTicker, IWallet brokerWallet,
        IBrokerMoneyProcessExtraDataService<W> externalData);
}