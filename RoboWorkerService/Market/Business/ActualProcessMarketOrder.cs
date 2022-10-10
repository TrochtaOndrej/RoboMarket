using System.Runtime.CompilerServices;
using Breaker.Helpers.Extensions;
using ExchangeSharp;
using NLog.LayoutRenderers.Wrappers;
using RoboWorkerService.Interface;
using RoboWorkerService.Market.Enum;
using RoboWorkerService.Market.Model;

namespace RoboWorkerService.Market.Business;

/// <summary> Definice kolik se ma pouzit pri transakci penez </summary>
public record DefinedMoneyProcessMarket : ActualProcessMarketOrder
{
    public override Type Type => GetType();

    public DefinedMoneyProcessMarket(ILogger<ActualProcessMarketOrder> logger, Wallet wallet) : base(logger, wallet)
    {
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

/// <summary>
/// Hrnkovy process zpracovani - Prelivani s jednoho hrnu na druhy a udrzovani stejne hodnoty po celou dobu
/// </summary>
public record CupProcessMarket : ActualProcessMarketOrder, ICupProcessMarketValue
{
    public override Type Type => GetType();
    private readonly ILogger<CupProcessMarket> _logger;

    public CupProcessMarket(ILogger<ActualProcessMarketOrder> baseLogger, ILogger<CupProcessMarket> logger, Wallet actualWallet) : base(baseLogger, actualWallet)
    {
        _logger = logger;
    }

    public MarketProcessBuyOrSell? CreateBuyOrder(decimal defineProfitEur)
    {
        Validation();
        decimal walletMarketPositionPriceInEur = GetWallet_EurToCryptoBuy();
        if (Wallet.EurAccountValue - walletMarketPositionPriceInEur == 0) return null;

        var diffEurToBuy = (Wallet.EurAccountValue - walletMarketPositionPriceInEur) / 2;
        var feesEurToBuy = CalculateFees(diffEurToBuy);
        var realProfitToBuy = diffEurToBuy - feesEurToBuy;

        _logger.LogInformation("PROFIT BUY -> Actual wallet: {actualWallet}, Define profit: {profit}, Actual profit {actual:F3}", Wallet.EurAccountValue, defineProfitEur, realProfitToBuy);

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

    public MarketProcessBuyOrSell? CreateSellOrder(decimal defineProfitEur)
    {
        Validation();
        decimal walletMarketSellPositionPriceInEur = GetWallet_EurToCryptoSell();
        if (walletMarketSellPositionPriceInEur - Wallet.EurAccountValue == 0) return null;

        var diffEurToSell = (walletMarketSellPositionPriceInEur - Wallet.EurAccountValue) / 2;
        if (diffEurToSell < 0) return null;

        var feesEurToSell = CalculateFees(diffEurToSell);
        var realProfitSell = diffEurToSell - feesEurToSell;

        _logger.LogInformation("PROFIT SELL -> Actual wallet: {wallet} Define profit: {profit}, Actual profit {actual:F3}", Wallet.EurAccountValue, defineProfitEur, realProfitSell);
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


/// <summary> Aktualni hodnota v marketu </summary>
public abstract record ActualProcessMarketOrder : MarketCrypto
{
    public abstract Type Type { get; }
    private readonly ILogger<ActualProcessMarketOrder> _logger;
    protected Wallet Wallet { get; }

    public ActualProcessMarketOrder(ILogger<ActualProcessMarketOrder> logger, Wallet wallet)
    {
        _logger = logger;
        Wallet = wallet;
    }

    /// <summary> Aktualni hodnota v bitcoinu v penezence prevedena na EUR pro Nakup</summary>
    public decimal GetWallet_EurToCryptoBuy()
    {
        Validation();
        return Wallet.CryptoAccountValue * CryptoPriceBuy;
    }

    /// <summary> Aktualni hodnota v bitcoinu v penezence prevedena na EUR pro Prodej</summary>
    public decimal GetWallet_EurToCryptoSell()
    {
        Validation();
        return Wallet.CryptoAccountValue * CryptoPriceSell;
    }

    public decimal GetWallet_EurToCrypto()
    {
        Validation();
        return (GetWallet_EurToCryptoBuy() + GetWallet_EurToCryptoSell()) / 2;
    }

    public void SetActualValue(ExchangeTicker ticker)
    {
        if (!string.Equals(Wallet.MarketSymbol, ticker.MarketSymbol, StringComparison.InvariantCultureIgnoreCase))
            throw new BussinesExceptions($" CryptoCurrency [{CryptoCurrency.ToString()}] is no the same with order market currency [{ticker.MarketSymbol}] !");
        CryptoCurrency = new CryptoCurrency(ticker.MarketSymbol);
        CryptoPriceSell = ticker.Bid;
        CryptoPriceBuy = ticker.Ask;
    }
    /// <summary> Investovane penize za nakup se vypocita fees </summary>
    /// <param name="investingValue"></param>
    /// <returns></returns>
    protected decimal CalculateFees(decimal investingValue)
    {
        decimal feesPercentlyInMarket = 0.6m; // Coinbase ma 0.6%
        return (investingValue / 100) * feesPercentlyInMarket;
    }

    public MarketProcessType GetActualMArketProcess()
    {
        // Penize na pozici - penezenka >=0 
        if (GetWallet_EurToCrypto() - Wallet.EurAccountValue >= 0) return MarketProcessType.Sell;
        return MarketProcessType.Buy;
    }


    protected void Validation()
    {
        if (Wallet.CryptoCurrency.ToString() != CryptoCurrency.ToString()) throw new BussinesExceptions(" Wallet marketCurrency is no the same with MarketValue!");
    }
}