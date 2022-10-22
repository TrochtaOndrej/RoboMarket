using ExchangeSharp;
using Helper.Serialization;
using RoboWorkerService.Config;
using RoboWorkerService.Interfaces;
using RoboWorkerService.Market.Enum;
using RoboWorkerService.Market.Model;

namespace RoboWorkerService.Market.Processing;

/// <summary> Definice kolik se ma pouzit pri transakci penez </summary>
public class SharpProcessingMarket<W> : BaseProcessMarketOrder<W>, IDefinedMoneyProcessMarket<W>
    where W : ICryptoCurrency
{
    private readonly ILogger<SharpProcessingMarket<W>> _logger;
    private readonly W _cryptoCurrency;


    public SharpProcessingMarket(
        ILogger<SharpProcessingMarket<W>> logger,
        IWallet<W> globalWallet,
        IConfig config,
        IJsonConvertor json,
        W cryptoCurrency)
        : base(logger, globalWallet, config, json, nameof(SharpProcessingMarket<W>))
    {
        _logger = logger;
        _cryptoCurrency = cryptoCurrency;
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

            // Zaokrouhlovani pozice na pocet des mist. Jinak chyba "message":"price is too accurate. Smallest unit is 0.01"
            positionPercentBuy = Math.Round(positionPercentBuy, _cryptoCurrency.CountDecimalNumberInPosition,
                MidpointRounding.ToEven);

            if (positionPercentBuy < 100)
                throw new BussinesExceptions("Buy position is less then 100. Please check the parameters in file or market!");

            var fees = CalculateFees(investMoneyEur);
            return new MarketProcessBuyOrSell(CryptoCurrency)
            {
                CryptoValue = investMoneyEur / positionPercentBuy,
                EurValue = investMoneyEur,
                ProcessType = MarketProcessType.Buy,
                Price = positionPercentBuy,
                Fees = fees,
                IsPostOnly = true,
                ProfitPercently = defineProfitInPercently,
                ProfitInEur = (investMoneyEur / 100) * defineProfitInPercently - fees,
            };
        }
        else if (marketProcessType == MarketProcessType.Sell)
        {
            var positionPercentSell = CryptoPriceSell + ((CryptoPriceSell / 100) * defineProfitInPercently); // hranice prodeje
            positionPercentSell = Math.Round(positionPercentSell, _cryptoCurrency.CountDecimalNumberInPosition,
                MidpointRounding.ToEven);

            if (CryptoPriceSell <= positionPercentSell)
            {
                var fees = CalculateFees(investMoneyEur);
                return new MarketProcessBuyOrSell(CryptoCurrency)
                {
                    CryptoValue = investMoneyEur / CryptoPriceSell,
                    EurValue = investMoneyEur,
                    ProcessType = MarketProcessType.Sell,
                    Price = positionPercentSell,
                    Fees = fees,
                    IsPostOnly = true,
                    ProfitPercently = defineProfitInPercently,
                    ProfitInEur = (investMoneyEur / 100) * defineProfitInPercently - fees,
                };
            }
        }

        return null;
    }

    //Z penezenky zjisti aktualni suu a propocita mrizku, dle definice
    public List<MarketProcessBuyOrSell> InicializationFirstSharpStrategy(ExchangeTicker firstTicker, IWallet brokerWallet,
        IBrokerMoneyProcessExtraDataService<W> externalData)
    {
// vypocet se zaklada na aktualnich hodnotach v marketu
        SetActualValueFromMarket(firstTicker); // pro vypocet strategie je treba zjistit aktulani hodnoty

        var listBuyOrSell = new List<MarketProcessBuyOrSell>();
        var buyData = externalData.MoneyProcessDataBuy;
        if (buyData.MarketProcessType == MarketProcessType.Buy)
        {
// TO BUY
            var countToBuys = (buyData.PercentSpectrumEnd - buyData.PercentSpectrumStart) / buyData.PercentStepCalculatePrice;
            var moneyStepToBuyEurPrice = buyData.PriceInEur / countToBuys;

            if (moneyStepToBuyEurPrice >= 1)
            {
                for (int i = 0; i < countToBuys - 1; i++)
                {
                    var positionPercentToBuy = buyData.PercentSpectrumStart + buyData.PercentStepCalculatePrice * i;
                    var buyOrSell = CreateBuyOrderEur(positionPercentToBuy, moneyStepToBuyEurPrice, MarketProcessType.Buy);
                    if (buyOrSell is not null)
                    {
                        listBuyOrSell.Add(buyOrSell);
                        buyData.PriceInEur -= moneyStepToBuyEurPrice;
                        if (buyData.PriceInEur <= 0) break;
                        // nejsou zadne penize v penezence
                    }
                    else
                        break;
                }
            }
            else
            {
                _logger.LogDebug("SHARP BUY: Calculated money is less then one Euro. Change the data:" +
                                 ObjectDumper.Dump(buyData));
            }
        }

// TO SELL
        var sellData = externalData.MoneyProcessDataSell;
        if (sellData.MarketProcessType == MarketProcessType.Sell)
        {
            var countToSell = (sellData.PercentSpectrumEnd - sellData.PercentSpectrumStart) / sellData.PercentStepCalculatePrice;
            var moneyStepToSellCryptoPrice = sellData.PriceInCrypto / countToSell;
            if (moneyStepToSellCryptoPrice >= 1)
            {
                for (int i = 0; i < countToSell - 1; i++)
                {
                    var positionPercentToSell = buyData.PercentSpectrumStart + buyData.PercentStepCalculatePrice * i;

                    var buyOrSell = CreateBuyOrderEur(positionPercentToSell, moneyStepToSellCryptoPrice, MarketProcessType.Sell);
                    if (buyOrSell is not null)
                    {
                        listBuyOrSell.Add(buyOrSell);
                        sellData.PriceInCrypto -= moneyStepToSellCryptoPrice;
                        if (sellData.PriceInCrypto <= 0) break; // uz neni zadne crypto v penezence
                    }
                    else
                        break;
                }
            }
            else
            {
                _logger.LogInformation("SHARP SELL: Calculated money is less then one Euro. Change the data:" +
                                       ObjectDumper.Dump(buyData));
            }
        }

        return listBuyOrSell;
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