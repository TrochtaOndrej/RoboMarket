using ExchangeSharp;
using RoboWorkerService.Interface;
using RoboWorkerService.Market.Enum;
using RoboWorkerService.Market.Model;

namespace RoboWorkerService.Market.Business;

public class CalculateCrypto : ICalculateCrypto
{
    private readonly Wallet _actualWallet;

    private readonly ILogger<CalculateCrypto> _logger;

    private const decimal AdditionValueToBuyOrSell = 1; // hodonta, ktera se pricita k pozici pro koupi nebo prodej 

    public CalculateCrypto(
        Wallet actualWallet,
        ILogger<CalculateCrypto> logger)
    {
        _actualWallet = actualWallet;
        _logger = logger;
    }
    public MarketProcessBuyOrSell GetOrderWithParameter(OrderProcessType orderProcessType, decimal profitPercent, decimal investmentPrice,
        IActualMarketValue actualMarketValue)
    {
        if (profitPercent < 0.01m && profitPercent > 100)
            throw new BussinesExceptions($"{nameof(profitPercent)} is out of range!");
        if (investmentPrice < 1) throw new BussinesExceptions("Investment money must > 1 Eur! ");

        switch (orderProcessType)
        {
            case OrderProcessType.Buy:
                //var cryptoValueBuy = actualMarketValue.GetWallet_EurToCryptoBuy(_actualWallet);
                var totalPriceWitProfit = investmentPrice + (investmentPrice / 100 * profitPercent);
               // var buyFees = CalculateFees(totalPriceWitProfit);
              //  var totalPriceToBuy = totalPriceWitProfit + buyFees;

                //  actualMarketValue.CryptoPriceBuy
                //Vypocet kdy nakoupit
                var oneEur = investmentPrice / actualMarketValue.CryptoPriceBuy;

                break;
        }

        return null;
    }

    /// <summary> Vypocita jestli se ma uskutecnit prodej nebo nakup. Pokud se nema nic provadet vraci NULL! </summary>
    /// <param name="profitEur"> Profit v EUR od ktereho se uskutecni proces</param>
    /// <returns>Pokud se vrati null nic se neprovadi. Neni zadny profit</returns>
    public MarketProcessBuyOrSell? CalculateSellOrBuy(decimal profitEur, IActualMarketValue actualMarketValue, decimal addExtraMoneyToPrice = 0)
    {
        if (actualMarketValue.MarketCurrency != _actualWallet.NameCurrency)
            throw new ArgumentOutOfRangeException("Type currency is not the same in wallet!");

        _logger.LogInformation("Actual market wallet {walletCrypto}: {btc} - {actualWalletEurBuy:F4}", _actualWallet.MarketSymbol, _actualWallet.CryptoAccountValue, actualMarketValue.GetWallet_EurToCrypto(_actualWallet));

        var actualProcessOrder = actualMarketValue.GetActualMArketProcess(_actualWallet);
        switch (actualProcessOrder)
        {
            case MarketProcessType.Buy:
                return actualMarketValue.CreateBuyOrder(_actualWallet, profitEur);
            case MarketProcessType.Sell:
                return actualMarketValue.CreateSellOrder(_actualWallet, profitEur);
        }

        return null;
    }
}