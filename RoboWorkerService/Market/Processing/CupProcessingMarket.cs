using ExchangeSharp;
using Helper.Serialization;
using RoboWorkerService.Config;
using RoboWorkerService.Interface;
using RoboWorkerService.Interfaces;
using RoboWorkerService.Market.Enum;
using RoboWorkerService.Market.Model;

namespace RoboWorkerService.Market.Processing;

public interface ICupProcessingMarket<W> : IProcessAllMarketOrder<W> where W : ICryptoCurrency
{
   
}

/// <summary>
/// Hrnkovy process zpracovani - Prelivani s jednoho hrnu na druhy a udrzovani stejne hodnoty po celou dobu
/// </summary>
public class CupProcessingMarket<W> : BaseProcessMarketOrder<W>, ICupProcessingMarket<W> where W : ICryptoCurrency
{
    private readonly ILogger<CupProcessingMarket<W>> _logger;

    public CupProcessingMarket(
        ILogger<CupProcessingMarket<W>> logger,
        IWallet<W> globalWallet,
        IConfig config,
        IJsonConvertor json
    ) : base(logger, globalWallet, config, json, nameof(CupProcessingMarket<W>))
    {
        _logger = logger;
    }

    public void CalculateGlobalWallet()
    {
        GlobalWallet.SumAllWalletAndInsertIntoGlobalWallet();
    }

    public MarketProcessBuyOrSell? RunProcessing(ExchangeTicker ticker)
    {
        //vypocti profit 
        SetActualValueFromMarket(ticker);
        return CalculateSellOrBuy(1M);
    }

    public void SaveWallet()
    {
        SaveWalletToFile();
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

    private MarketProcessBuyOrSell? CreateBuyOrder(decimal defineProfitEur)
    {
        Validation();
        decimal walletMarketPositionPriceInEur = GetWallet_EurToCryptoBuy();
        if (BrokerWallet.EurAccountValue - walletMarketPositionPriceInEur == 0) return null;

        var diffEurToBuy = (BrokerWallet.EurAccountValue - walletMarketPositionPriceInEur) / 2;
        var feesEurToBuy = CalculateFees(diffEurToBuy);
        var realProfitToBuy = diffEurToBuy - feesEurToBuy;

        _logger.LogInformation(
            "PROFIT BUY -> Actual Broker Wallet: {actualWallet}, Define profit: {profit}, Actual profit {actual:F3}",
            BrokerWallet.EurAccountValue, defineProfitEur, realProfitToBuy);

        if (realProfitToBuy > defineProfitEur)
        {
            if (diffEurToBuy < 0.86m) return null; // minimalni nakup
            return new MarketProcessBuyOrSell(CryptoCurrency)
            {
                CryptoValue = Math.Abs(diffEurToBuy / CryptoPriceBuy),
                EurValue = Math.Abs(diffEurToBuy),
                ProcessType = MarketProcessType.Buy,
                Price = CryptoPriceBuy,
                Fees = feesEurToBuy
            };
        }

        return null;
    }

    private MarketProcessBuyOrSell? CreateSellOrder(decimal defineProfitEur)
    {
        Validation();
        decimal walletMarketSellPositionPriceInEur = GetWallet_EurToCryptoSell();
        if (walletMarketSellPositionPriceInEur == 0 || BrokerWallet.EurAccountValue == 0)
        {
            _logger.LogWarning("GlobalWallet is empty - Check the value or set inicialization values for GlobalWallet");
            //TODO OT: je potreba nekde varovat Aktualni penezenka je prazdna a obchodovani nelze provest
            return null;
        }

        if (walletMarketSellPositionPriceInEur - BrokerWallet.EurAccountValue == 0) return null;

        var diffEurToSell = (walletMarketSellPositionPriceInEur - BrokerWallet.EurAccountValue) / 2;
        if (diffEurToSell < 0) return null;

        var feesEurToSell = CalculateFees(diffEurToSell);
        var realProfitSell = diffEurToSell - feesEurToSell;

        _logger.LogInformation(
            "PROFIT SELL -> Actual broker Wallet: {globalWallet} Define profit: {profit}, Actual profit {actual:F3}",
            BrokerWallet.EurAccountValue, defineProfitEur, realProfitSell);
        if (realProfitSell > defineProfitEur)
        {
            if (diffEurToSell < 0.86m) return null; // minimalni nakup

            return new MarketProcessBuyOrSell(CryptoCurrency)
            {
                CryptoValue = Math.Abs(diffEurToSell / CryptoPriceBuy),
                EurValue = Math.Abs(diffEurToSell),
                ProcessType = MarketProcessType.Sell,
                Price = CryptoPriceBuy,
                Fees = feesEurToSell
            };
        }

        return null;
    }
}