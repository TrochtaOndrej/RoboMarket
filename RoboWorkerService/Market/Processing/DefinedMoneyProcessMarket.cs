using ExchangeSharp;
using Helper.Serialization;
using RoboWorkerService.Config;
using RoboWorkerService.Interfaces;
using RoboWorkerService.Market.Enum;
using RoboWorkerService.Market.Model;

namespace RoboWorkerService.Market.Processing;

/// <summary> Definice kolik se ma pouzit pri transakci penez </summary>
public class DefinedMoneyProcessMarket<W> : BaseProcessMarketOrder<W>, IDefinedMoneyProcessMarket<W>
    where W : ICryptoCurrency
{
    private readonly ILogger<DefinedMoneyProcessMarket<W>> _logger;


    public DefinedMoneyProcessMarket(
        ILogger<DefinedMoneyProcessMarket<W>> logger,
        IWallet<W> globalWallet,
        IConfig config,
        IJsonConvertor json)
        : base(logger, globalWallet, config, json, nameof(DefinedMoneyProcessMarket<W>))
    {
        _logger = logger;
    }

    #region Wallet

    public void CalculateGlobalWallet()
    {
        GlobalWallet.SumAllWalletAndInsertIntoGlobalWallet();
    }

    public async Task<List<MarketProcessBuyOrSell>> RunProcessingAsync(ExchangeTicker ticker,
        IBrokerMoneyProcessExtraDataService<W> extraDataService,
        List<ExchangeOrderResult> openOrdersActualInMarket)
    {
        SetActualValueFromMarket(ticker);
        return await CompareSavedOrderWithMarketOrderAndGetNewOrderBuyOrSellAsync(extraDataService, openOrdersActualInMarket);
    }

    public void SaveWallet()
    {
        SaveWalletToFile();
    }

    #endregion

    #region Processing Order - Strategy #

    public MarketProcessBuyOrSell? CreateBuyOrderCryptoCurrency(decimal defineProfitInPercently, decimal cryptoMoney,
        MarketProcessType marketProcessType)
    {
        if (defineProfitInPercently < 0.6m)
            throw new BussinesExceptions("Profit must by more then 0.6%. This percent is for market feeds");

        if (BrokerWallet.CryptoPositionTransaction < 100)
            throw new BussinesExceptions(
                $"Actual BrokerWallet.CryptoPositionTransaction is less 100. PLease fill the position in wallet. {FileName}");

        if (marketProcessType == MarketProcessType.Buy)
        {
            var positionPercentBuy = CryptoPriceBuy - (CryptoPriceBuy / 100 * defineProfitInPercently); // hranice nakupu

            return new MarketProcessBuyOrSell(CryptoCurrency)
            {
                CryptoValue = cryptoMoney,
                EurValue = cryptoMoney * positionPercentBuy,
                ProcessType = MarketProcessType.Buy,
                Price = positionPercentBuy,
                Fees = CalculateFees(cryptoMoney * positionPercentBuy),
                IsPostOnly = true
            };
        }
        else if (marketProcessType == MarketProcessType.Sell)
        {
            var positionPercentSell = CryptoPriceSell + ((CryptoPriceSell / 100) * defineProfitInPercently); // hranice prodeje

            if (CryptoPriceSell <= positionPercentSell)
            {
                return new MarketProcessBuyOrSell(CryptoCurrency)
                {
                    CryptoValue = cryptoMoney / CryptoPriceSell,
                    EurValue = cryptoMoney,
                    ProcessType = MarketProcessType.Sell,
                    Price = positionPercentSell,
                    Fees = CalculateFees(cryptoMoney),
                    IsPostOnly = true,
                };
            }
        }

        return null;
    }

    public MarketProcessBuyOrSell? CreateBuyOrderEur(decimal defineProfitInPercently, decimal investMoneyEur,
        MarketProcessType marketProcessType)
    {
        if (defineProfitInPercently < 0.6m)
            throw new BussinesExceptions("Profit must by more then 0.6%. This percent is for market feeds");

        if (BrokerWallet.CryptoPositionTransaction < 100)
            throw new BussinesExceptions(
                $"Actual BrokerWallet.CryptoPositionTransaction is less 100. PLease fill the position in wallet. {FileName}");

        if (marketProcessType == MarketProcessType.Buy)
        {
            var positionPercentBuy = CryptoPriceBuy - (CryptoPriceBuy / 100 * defineProfitInPercently); // hranice nakupu

            return new MarketProcessBuyOrSell(CryptoCurrency)
            {
                CryptoValue = investMoneyEur / CryptoPriceBuy,
                EurValue = investMoneyEur,
                ProcessType = MarketProcessType.Buy,
                Price = positionPercentBuy,
                Fees = CalculateFees(investMoneyEur),
                IsPostOnly = true
            };
        }
        else if (marketProcessType == MarketProcessType.Sell)
        {
            var positionPercentSell = CryptoPriceSell + ((CryptoPriceSell / 100) * defineProfitInPercently); // hranice prodeje

            if (CryptoPriceSell <= positionPercentSell)
            {
                return new MarketProcessBuyOrSell(CryptoCurrency)
                {
                    CryptoValue = investMoneyEur / CryptoPriceSell,
                    EurValue = investMoneyEur,
                    ProcessType = MarketProcessType.Sell,
                    Price = positionPercentSell,
                    Fees = CalculateFees(investMoneyEur),
                    IsPostOnly = true,
                };
            }
        }

        return null;
    }

    //Z penezenky zjisti aktualni suu a propocita mrizku, dle definice
    public List<MarketProcessBuyOrSell> InicializationFirstSharpStrategy(ExchangeTicker firstTicker, IWallet brokerWallet,
        IBrokerMoneyProcessExtraDataService<W> externalData)
    {
        var listBuyOrSell = new List<MarketProcessBuyOrSell>();

        var buyData = externalData.MoneyProcessDataBuy;
        if (buyData.MarketProcessType == MarketProcessType.Buy)
        {
// TO BUY
            var countToBuys = (buyData.PercentSpectrumStart - buyData.PercentSpectrumEnd) / buyData.PercentStepCalculatePrice;
            var moneyStepToBuyEurPrice = buyData.PriceInEur / countToBuys;

            if (moneyStepToBuyEurPrice < 1)
                throw new BussinesExceptions(" Calculated money is less then one Euro. Change the data:" +
                                             ObjectDumper.Dump(buyData));

            for (int i = 0; i < countToBuys - 1; i++)
            {
                var positionPercentToBuy = buyData.PercentSpectrumStart + buyData.PercentStepCalculatePrice * i;
                var buyOrSell = CreateBuyOrderEur(positionPercentToBuy, moneyStepToBuyEurPrice, MarketProcessType.Buy);
                if (buyOrSell is not null)
                {
                    listBuyOrSell.Add(buyOrSell);
                    buyData.PriceInEur -= moneyStepToBuyEurPrice;
                    if (buyData.PriceInEur < 0) break;; // nejsou zadne penize v penezence
                }
                else
                    break;
            }
        }

// TO SELL
        var sellData = externalData.MoneyProcessDataSell;
        if (sellData.MarketProcessType == MarketProcessType.Sell)
        {
            var countToSell = (sellData.PercentSpectrumStart - sellData.PercentSpectrumEnd) / sellData.PercentStepCalculatePrice;
            var moneyStepToSellCryptoPrice = sellData.PriceInCrypto / countToSell;
            for (int i = 0; i < countToSell - 1; i++)
            {
                var positionPercentToBuy = buyData.PercentSpectrumStart + buyData.PercentStepCalculatePrice * i;
                
                var buyOrSell = CreateBuyOrderEur(positionPercentToBuy, moneyStepToSellCryptoPrice, MarketProcessType.Sell);
                if (buyOrSell is not null)
                {
                    listBuyOrSell.Add(buyOrSell);
                    sellData.PriceInCrypto -= moneyStepToSellCryptoPrice;
                    if (sellData.PriceInCrypto < 0) break;// uz neni zadne crypto v penezence
                }
                else
                    break;
            }
        }
    }


    /// <summary> Porovna aktualni ulozene ordery s ordery na Marketu. Pokud jsou nejake uzavrene
    /// ordery, vytvor dalsi order ze ziskem a vymaz order z extraData listu </summary>
    private async Task<List<MarketProcessBuyOrSell>> CompareSavedOrderWithMarketOrderAndGetNewOrderBuyOrSellAsync(
        IBrokerMoneyProcessExtraDataService<W> extraDataService,
        List<ExchangeOrderResult> openOrdersActualInMarket)
    {
        var orderBuyOrSellList = new List<MarketProcessBuyOrSell>();
        var savedOrderTransaction = extraDataService.GetOpenOrderTransaction().ToList();
        if (!savedOrderTransaction.Any())
        {
            _logger.LogInformation("No open transaction in {Crypto}", Crypto);
            return orderBuyOrSellList;
        }

        foreach (var actualSavedOrderTransaction in savedOrderTransaction)
        {
            var actualMarketOpenOrder = openOrdersActualInMarket.FirstOrDefault(x =>
                x.TradeId == actualSavedOrderTransaction.OrderResult.TradeId &&
                x.OrderId == actualSavedOrderTransaction.OrderResult.OrderId);
            if (actualMarketOpenOrder == null) continue;

            if (actualSavedOrderTransaction.OrderResult.Result != actualMarketOpenOrder.Result &&
                actualMarketOpenOrder.Result.IsCompleted())
            {
                // doslo ke zmene transakce (vykonal se nakup, nebo se zrusil nakup a pod.
                _logger.LogInformation("Actual order is completed. Transaction: {@Order}", actualMarketOpenOrder);
                //je treba vytvorit objednavku se ziskem
                MarketProcessBuyOrSell? orderBuyOrSell = CreateBuyOrderEur(1.2m, actualMarketOpenOrder.Amount,
                    actualMarketOpenOrder.IsBuy ? MarketProcessType.Sell : MarketProcessType.Buy);

                if (orderBuyOrSell is not null) orderBuyOrSellList.Add(orderBuyOrSell);

                // odstran jiz stavajici order v extradata
                extraDataService.RemoveTransaction(actualSavedOrderTransaction);
            }
        }

        await extraDataService.SaveDataAsync();
        return orderBuyOrSellList;
    }

    #endregion
}