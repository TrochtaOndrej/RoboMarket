using ExchangeSharp;
using Helper.Interface;
using Helper.Serialization;
using RoboWorkerService.Config;
using RoboWorkerService.Interface;
using RoboWorkerService.Interfaces;
using RoboWorkerService.Market.Enum;
using RoboWorkerService.Market.Model;

namespace RoboWorkerService.Market.Processing;

/// <summary> Hrnkovy process zpracovani - Prelivani s jednoho hrnu na druhy a udrzovani stejne hodnoty po celou dobu </summary>
public class CupProcessingMarket<W> : BaseProcessMarketOrder<W>, ICupProcessingMarket<W> where W : ICryptoCurrency
{
    private readonly ILogger<CupProcessingMarket<W>> _logger;

    public CupProcessingMarket(
        ILogger<CupProcessingMarket<W>> logger,
        IWallet<W> globalWallet,
        IConfig config,
        IJsonConvertor json,
        IAppRobo appRobo,
        W type
    ) : base(type, logger, globalWallet, config, json, appRobo, nameof(CupProcessingMarket<W>))
    {
        _logger = logger;
    }

    public void CalculateGlobalWallet()
    {
        GlobalWallet.SumAllWalletAndInsertIntoGlobalWallet();
    }

    public MarketProcessBuyOrSell? RunProcessing(ExchangeTicker ticker)
    {
        // zatim nei implementovano
        return null;
        //vypocti profit 
        SetActualValueFromMarket(ticker);
        return CalculateSellOrBuy(1.5M);
    }

    public Task SaveWalletAsync()
    {
        return SaveWalletToFileAsync();
    }

    /// <summary> Vypocita jestli se ma uskutecnit prodej nebo nakup. Pokud se nema nic provadet vraci NULL! </summary>
    /// <param name="profitEur"> Profit v EUR od ktereho se uskutecni proces</param>
    /// <returns>Pokud se vrati null nic se neprovadi. Neni zadny profit</returns>
    private MarketProcessBuyOrSell? CalculateSellOrBuy(decimal profitEur)
    {
        if (!CryptoCurrency.Equals(GlobalWallet.CryptoCurrency))
            throw new ArgumentOutOfRangeException("Type currency is not the same in globalWallet!");

        _logger.LogInformation("Actual market globalWallet {walletCrypto}: {btc} - {actualWalletEurBuy:F4}",
            BrokerWallet.MarketSymbol, BrokerWallet.CryptoAccountValue, GetWallet_EurToCrypto());

        var actualProcessOrder = GetActualMArketProcess();
        switch (actualProcessOrder)
        {
            case MarketProcessType.Buy:
                return CreateBuyOrder(profitEur);
            case MarketProcessType.Sell:
                return CreateSellOrder(profitEur);
        }

        return null;
    }

    public MarketProcessType GetActualMArketProcess()
    {
        // Penize na pozici - penezenka >=0 
        if (GetWallet_EurToCrypto() - BrokerWallet.EurAccountValue >= 0) return MarketProcessType.Sell;
        return MarketProcessType.Buy;
    }

    private MarketProcessBuyOrSell? CreateBuyOrder(decimal defineProfitEur)
    {
        return null;
    }


    private MarketProcessBuyOrSell? CreateSellOrder(decimal positionCrypto, decimal definePercentProfit = 1.0m,
        decimal investMoneyInEur = 1.0m)
    {
        Validation();
        if (definePercentProfit < 0.6m) return null;
        if (investMoneyInEur < 1) return null;
        if (GetWallet_EurToCryptoSell() == 0 || BrokerWallet.EurAccountValue == 0)
        {
            _logger.LogWarning("GlobalWallet is empty - Check the value or set inicialization values for GlobalWallet");
            return null;
        }

        base.CreateBuyOrderEur(definePercentProfit, investMoneyInEur, MarketProcessType.Sell, positionCrypto);

        return null;
    }
}