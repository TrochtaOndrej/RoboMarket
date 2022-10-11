using ExchangeSharp;
using Helper.Serialization;
using RoboWorkerService.Config;
using RoboWorkerService.Market.Enum;
using RoboWorkerService.Market.Model;

namespace RoboWorkerService.Market.Processing;

public interface IDefinedMoneyProcessMarket<W> : IBaseProcessMarketOrder<W> where W : ICryptoCurrency
{
    MarketProcessBuyOrSell? RunProcessing(ExchangeTicker ticker);
}

/// <summary> Definice kolik se ma pouzit pri transakci penez </summary>
public class DefinedMoneyProcessMarket<W> : BaseProcessMarketOrder<W>, IDefinedMoneyProcessMarket<W> where W : ICryptoCurrency
{

    public DefinedMoneyProcessMarket(
        ILogger<BaseProcessMarketOrder<W>> logger,
        IWallet<W> wallet,
        IConfig config,
        IJsonConvertor json
    ) : base(logger, wallet, config, json)
    {
    }

    public MarketProcessBuyOrSell? RunProcessing(ExchangeTicker ticker)
    {
        return CreateBuyOrder(1, 100);
    }

    public MarketProcessBuyOrSell? CreateBuyOrder(decimal defineProfitInPercently, decimal investMoneyEur)
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

        var positionPercentSell = Wallet.CryptoPositionTransaction / 100 * defineProfitInPercently + CryptoPriceSell; // hranice prodeje

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
}