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
    private readonly IWallet<W, IDefinedMoneyProcessMarket<W>> _walletLocal;

    public DefinedMoneyProcessMarket(
        ILogger<BaseProcessMarketOrder<W>> logger,
        IWallet<W> wallet,
        IConfig config,
        IJsonConvertor json,
        IWallet<W,IDefinedMoneyProcessMarket<W>> walletLocal) : base(logger, wallet, config, json, nameof(DefinedMoneyProcessMarket<W>))
    {
        _walletLocal = walletLocal;
    }

    public MarketProcessBuyOrSell? RunProcessing(ExchangeTicker ticker)
    {
        SetActualValueFromMarket(ticker);
        return CreateBuyOrder(1, 100);
    }

    private MarketProcessBuyOrSell? CreateBuyOrder(decimal defineProfitInPercently, decimal investMoneyEur)
    {
        if (defineProfitInPercently < 0.6m)
            throw new BussinesExceptions("Profit must by more then 0.6%. This percent is for market feeds");

        var positionPercentBuy = Wallet.CryptoPositionTransaction / 100 * defineProfitInPercently + CryptoPriceBuy; // hranice nakupu

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

        var positionPercentSell = CryptoPriceSell -(Wallet.CryptoPositionTransaction / 100) * defineProfitInPercently  ; // hranice prodeje

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