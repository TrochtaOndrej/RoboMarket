using ExchangeSharp;
using Helper.Interface;
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
        IAppRobo appRobo,
        W type)
        : base(type, logger, globalWallet, config, json, appRobo, nameof(SharpProcessingMarket<W>))
    {
        _logger = logger;
        _cryptoCurrency = type;
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

    public Task SaveWalletAsync()
    {
        return SaveWalletToFileAsync();
    }

    #endregion

    #region Processing Order - Strategy #

    public MarketProcessBuyOrSell? CreateBuyOrderCryptoCurrency(decimal defineProfitInPercently, decimal cryptoMoney,
        MarketProcessType marketProcessType, decimal? cryptoPosition = null)
    {
        if (defineProfitInPercently < 0.6m)
            throw new BussinesExceptions("Profit must by more then 0.6%. This percent is for market feeds");

        if (BrokerWallet.CryptoPositionTransaction <= 0)
            throw new BussinesExceptions(
                $"Actual BrokerWallet.CryptoPositionTransaction is less 0. PLease fill the position in wallet. {FileName}");

        // pokud neni definovana pozice tak , se bere aktualni pozice v marketu jinak se nastavi hodnota, kterou
        // definujeme v promene. Tahle podminka jen nastavuje pro vypocet hodnotu predanou
        if (cryptoPosition is not null)
        {
            CryptoPriceBuy = CryptoPriceSell = cryptoPosition.Value;
        }

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
                _logger.LogDebug($"SHARP BUY {_cryptoCurrency.Crypto}: Calculated money is less then one Euro. Change the data:" +
                                 ObjectDumper.Dump(buyData));
            }
        }

// TO SELL
        var sellData = externalData.MoneyProcessDataSell;
        if (sellData.MarketProcessType == MarketProcessType.Sell)
        {
            var countToSell = (sellData.PercentSpectrumEnd - sellData.PercentSpectrumStart) / sellData.PercentStepCalculatePrice;
            var moneyStepToSellCryptoPrice = sellData.PriceInCryptoInEur / countToSell;
            if (moneyStepToSellCryptoPrice >= 1)
            {
                for (int i = 0; i < countToSell - 1; i++)
                {
                    var positionPercentToSell = buyData.PercentSpectrumStart + buyData.PercentStepCalculatePrice * i;

                    var buyOrSell = CreateBuyOrderEur(positionPercentToSell, moneyStepToSellCryptoPrice, MarketProcessType.Sell);
                    if (buyOrSell is not null)
                    {
                        listBuyOrSell.Add(buyOrSell);
                        sellData.PriceInCryptoInEur -= moneyStepToSellCryptoPrice;
                        if (sellData.PriceInCryptoInEur <= 0) break; // uz neni zadne crypto v penezence
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
    /// ordery, vytvor dalsi order ze ziskem a vymazes stary order z order z extraData listu </summary>
    private async Task<List<MarketProcessBuyOrSell>> CompareSavedOrderWithMarketOrderAndGetNewOrderBuyOrSellAsync(
        IBrokerMoneyProcessExtraDataService<W> extraDataService,
        List<ExchangeOrderResult> openOrdersActualInMarket)
    {
        var orderBuyOrSellList = new List<MarketProcessBuyOrSell>();
        var savedLocalOrderTransaction = extraDataService.GetOpenOrderTransaction().ToList();
        if (!savedLocalOrderTransaction.Any())
        {
            _logger.LogInformation("No open transaction in {Crypto}", Crypto);
            return orderBuyOrSellList;
        }

        foreach (var actualSavedOrderTransaction in savedLocalOrderTransaction)
        {
            var actualMarketOpenOrder = openOrdersActualInMarket.FirstOrDefault(x =>
                x.OrderId == actualSavedOrderTransaction.OrderResult.OrderId);
            if (actualMarketOpenOrder == null) continue;

            if (actualSavedOrderTransaction.OrderResult.Result != actualMarketOpenOrder.Result &&
                actualMarketOpenOrder.Result.IsCompleted())
            {
                // doslo ke zmene transakce (vykonal se nakup, nebo se zrusil nakup a pod.
                _logger.LogInformation("Actual order is completed. Transaction: {@Order}", actualMarketOpenOrder);
                //je treba vytvorit objednavku se ziskem 
                // TODO OT: Dodelat lepsi strategii pro znovu nakup nebo prodej, Nejelepe rozdelit zisk na pulku a precentrulane rozpocitat
                MarketProcessBuyOrSell? orderBuyOrSell = CreateBuyOrderEur(actualSavedOrderTransaction.BuyOrSell.ProfitPercently,
                    actualSavedOrderTransaction.BuyOrSell
                        .EurValue, // TODO OT: vysoka priorita Vypocet nakupu nebo prodeje v pozici bez ztraty penez
                    actualMarketOpenOrder.IsBuy ? MarketProcessType.Sell : MarketProcessType.Buy,
                    actualMarketOpenOrder.Price);


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