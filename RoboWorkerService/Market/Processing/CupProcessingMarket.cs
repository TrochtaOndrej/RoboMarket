using ExchangeSharp;
using Helper.Serialization;
using RoboWorkerService.Config;
using RoboWorkerService.Market.Enum;
using RoboWorkerService.Market.Model;

namespace RoboWorkerService.Market.Processing;

public interface ICupProcessingMarket<W> : IBaseProcessMarketOrder<W> where W : ICryptoCurrency
{
    MarketProcessBuyOrSell? RunProcessing(ExchangeTicker ticker);
    void Init();
    void SaveWallet();
}

/// <summary>
/// Hrnkovy process zpracovani - Prelivani s jednoho hrnu na druhy a udrzovani stejne hodnoty po celou dobu
/// </summary>
public class CupProcessingMarket<W> : BaseProcessMarketOrder<W>, ICupProcessingMarket<W> where W : ICryptoCurrency
{
    private readonly ILogger<CupProcessingMarket<W>> _logger;

    public CupProcessingMarket(
        ILogger<CupProcessingMarket<W>> logger,
        IWallet<W> wallet,
        IConfig config,
        IJsonConvertor json
    ) : base(logger, wallet, config, json)
    {
        _logger = logger;
    }

    public MarketProcessBuyOrSell? RunProcessing(ExchangeTicker ticker)
    {
        //vypocti profit 
        SetActualValue(ticker);
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
        if (!CryptoCurrency.Equals(Wallet.CryptoCurrency))
            throw new ArgumentOutOfRangeException("Type currency is not the same in wallet!");

        _logger.LogInformation("Actual market wallet {walletCrypto}: {btc} - {actualWalletEurBuy:F4}",
            Wallet.MarketSymbol, Wallet.CryptoAccountValue, GetWallet_EurToCrypto());

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
        if (Wallet.EurAccountValue - walletMarketPositionPriceInEur == 0) return null;

        var diffEurToBuy = (Wallet.EurAccountValue - walletMarketPositionPriceInEur) / 2;
        var feesEurToBuy = CalculateFees(diffEurToBuy);
        var realProfitToBuy = diffEurToBuy - feesEurToBuy;

        _logger.LogInformation(
            "PROFIT BUY -> Actual wallet: {actualWallet}, Define profit: {profit}, Actual profit {actual:F3}",
            Wallet.EurAccountValue, defineProfitEur, realProfitToBuy);

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
        if (walletMarketSellPositionPriceInEur == 0 || Wallet.EurAccountValue == 0)
        {
            _logger.LogWarning("Wallet is empty - Check the value or set inicialization values for Wallet");
        }

        if (walletMarketSellPositionPriceInEur - Wallet.EurAccountValue == 0) return null;

        var diffEurToSell = (walletMarketSellPositionPriceInEur - Wallet.EurAccountValue) / 2;
        if (diffEurToSell < 0) return null;

        var feesEurToSell = CalculateFees(diffEurToSell);
        var realProfitSell = diffEurToSell - feesEurToSell;

        _logger.LogInformation(
            "PROFIT SELL -> Actual wallet: {wallet} Define profit: {profit}, Actual profit {actual:F3}",
            Wallet.EurAccountValue, defineProfitEur, realProfitSell);
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