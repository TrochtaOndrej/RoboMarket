using Breaker.Helpers.Extensions;
using ExchangeSharp;
using RoboWorkerService.Interface;
using RoboWorkerService.Market.Enum;
using RoboWorkerService.Market.Model;

namespace RoboWorkerService.Market.Business;

/// <summary> Aktualni hodnota v marketu </summary>
public record ActualMarketOrder : MarketCrypto, IActualMarketValue
{
    private readonly ILogger<ActualMarketOrder> _logger;

    public ActualMarketOrder(ILogger<ActualMarketOrder> logger)
    {
        _logger = logger;
    }

    /// <summary> Aktualni hodnota v bitcoinu v penezence prevedena na EUR pro Nakup</summary>
    public decimal GetWallet_EurToCryptoBuy(Wallet w)
    {
        Validation(w);
        return w.CryptoAccountValue * CryptoPriceBuy;
    }

    /// <summary> Aktualni hodnota v bitcoinu v penezence prevedena na EUR pro Prodej</summary>
    public decimal GetWallet_EurToCryptoSell(Wallet w)
    {
        Validation(w);
        return w.CryptoAccountValue * CryptoPriceSell;
    }

    public decimal GetWallet_EurToCrypto(Wallet w)
    {
        Validation(w);
        return (GetWallet_EurToCryptoBuy(w) + GetWallet_EurToCryptoSell(w)) / 2;
    }

    public void SetActualValue(ExchangeTicker ticker)
    {
        if (!string.Equals(MarketCurrency.GetEnumDescription(), ticker.MarketSymbol, StringComparison.InvariantCultureIgnoreCase))
            throw new BussinesExceptions($" {nameof(ActualMarketOrder)} is no the same with order market currency!");

        CryptoPriceSell = ticker.Bid;
        CryptoPriceBuy = ticker.Ask;
    }
    /// <summary> Investovane penize za nakup se vypocita fees </summary>
    /// <param name="investingValue"></param>
    /// <returns></returns>
    public static decimal CalculateFees(decimal investingValue)
    {
        decimal feesPercentlyInMarket = 0.6m; // Coinbase ma 0.6%
        return (investingValue / 100) * feesPercentlyInMarket;
    }

    public MarketProcessType GetActualMArketProcess(Wallet actualWallet)
    {
        // Penize na pozici - penezenka >=0 
        if (GetWallet_EurToCrypto(actualWallet) - actualWallet.EurAccountValue >= 0) return MarketProcessType.Sell;
        return MarketProcessType.Buy;
    }

    public MarketProcessBuyOrSell? CreateBuyOrder(Wallet actualWallet, decimal defineProfitEur)
    {
        Validation(actualWallet);
        decimal walletMarketPositionPriceInEur = GetWallet_EurToCryptoBuy(actualWallet);
        if (actualWallet.EurAccountValue - walletMarketPositionPriceInEur == 0) return null;

        var diffEurToBuy = (actualWallet.EurAccountValue - walletMarketPositionPriceInEur) / 2;
        var feesEurToBuy = CalculateFees(diffEurToBuy);
        var realProfitToBuy = diffEurToBuy - feesEurToBuy;

        _logger.LogInformation("PROFIT BUY -> Actual wallet: {actualWallet}, Define profit: {profit}, Actual profit {actual:F3}", actualWallet.EurAccountValue, defineProfitEur, realProfitToBuy);

        if (realProfitToBuy > defineProfitEur)
        {
            if (diffEurToBuy < 0.86m) return null; // minimalni nakup
            return new MarketProcessBuyOrSell(MarketCurrency)
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

    public MarketProcessBuyOrSell? CreateSellOrder(Wallet actualWallet, decimal defineProfitEur)
    {
        Validation(actualWallet);
        decimal walletMarketSellPositionPriceInEur = GetWallet_EurToCryptoSell(actualWallet);
        if (walletMarketSellPositionPriceInEur - actualWallet.EurAccountValue == 0) return null;

        var diffEurToSell = (walletMarketSellPositionPriceInEur - actualWallet.EurAccountValue) / 2;
        if (diffEurToSell < 0) return null;

        var feesEurToSell = CalculateFees(diffEurToSell);
        var realProfitSell = diffEurToSell - feesEurToSell;

        _logger.LogInformation("PROFIT SELL -> Actual wallet: {wallet} Define profit: {profit}, Actual profit {actual:F3}", actualWallet.EurAccountValue, defineProfitEur, realProfitSell);
        if (realProfitSell > defineProfitEur)
        {
            if (diffEurToSell < 0.86m) return null; // minimalni nakup

            return new MarketProcessBuyOrSell(MarketCurrency)
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

    private void Validation(Wallet w)
    {
        if (w.NameCurrency != MarketCurrency) throw new BussinesExceptions(" Wallet marketCurrency is no the same with MarketValue!");
    }
}