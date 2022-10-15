using ExchangeSharp;
using Helper.Serialization;
using RoboWorkerService.Config;
using RoboWorkerService.Interfaces;
using RoboWorkerService.Market.Enum;
using RoboWorkerService.Market.Model;

namespace RoboWorkerService.Market.Processing;

public interface IDefinedMoneyProcessMarket<W> : IProcessAllMarketOrder<W> where W : ICryptoCurrency
{
    MarketProcessBuyOrSell? RunProcessing(ExchangeTicker ticker);
}

/// <summary> Definice kolik se ma pouzit pri transakci penez </summary>
public class DefinedMoneyProcessMarket<W> : BaseProcessMarketOrder<W>, IDefinedMoneyProcessMarket<W> where W : ICryptoCurrency
{
 
    public DefinedMoneyProcessMarket(
        ILogger<BaseProcessMarketOrder<W>> logger,
        IWallet<W> globalWallet,
        IConfig config,
        IJsonConvertor json)
       : base(logger, globalWallet, config, json, nameof(DefinedMoneyProcessMarket<W>))
    { }

    public void CalculateGlobalWallet()
    {
       GlobalWallet.SumAllWalletAndInsertIntoGlobalWallet();
    }

    public MarketProcessBuyOrSell? RunProcessing(ExchangeTicker ticker)
    {
        return null; // TODO DORESIT NAPUP A PRODEJ (strategie mrizka)
        SetActualValueFromMarket(ticker);
        return CreateBuyOrder(1, 10);
    }

    private MarketProcessBuyOrSell? CreateBuyOrder(decimal defineProfitInPercently, decimal investMoneyEur)
    {
        if (defineProfitInPercently < 0.6m)
            throw new BussinesExceptions("Profit must by more then 0.6%. This percent is for market feeds");

        if (BrokerWallet.CryptoPositionTransaction < 100)
            throw new BussinesExceptions($"Actual BrokerWallet.CryptoPositionTransaction is less 100. PLease fill the position in wallet. {FileName} ");

        var positionPercentBuy = BrokerWallet.CryptoPositionTransaction / 100 * defineProfitInPercently + CryptoPriceBuy; // hranice nakupu

        if (CryptoPriceBuy >= positionPercentBuy)
        {
            return new MarketProcessBuyOrSell(CryptoCurrency)
            {
                CryptoValue = investMoneyEur / CryptoPriceBuy,
                EurValue = investMoneyEur,
                ProcessType = MarketProcessType.Sell,
                Price = CryptoPriceBuy,
                Fees = CalculateFees(investMoneyEur)
            };
        }

        var positionPercentSell = CryptoPriceSell -(BrokerWallet.CryptoPositionTransaction / 100) * defineProfitInPercently  ; // hranice prodeje

        if (CryptoPriceSell <= positionPercentSell)
        {
            return new MarketProcessBuyOrSell(CryptoCurrency)
            {
                CryptoValue = investMoneyEur / CryptoPriceBuy,
                EurValue = investMoneyEur,
                ProcessType = MarketProcessType.Buy,
                Price = CryptoPriceBuy,
                Fees = CalculateFees(investMoneyEur)
            };
        }

        return null;
    }

    public void SaveWallet()
    {
        SaveWalletToFile();
    }
}