﻿using ExchangeSharp;
using RoboWorkerService.Config;
using RoboWorkerService.Interfaces;
using RoboWorkerService.Market.Enum;
using RoboWorkerService.Market.Model;
using RoboWorkerService.Market.Processing;
using RoboWorkerService.Robo;

namespace RoboWorkerService.Market;

/// <summary> Jedna se o UZAVIREJ PREDEM DEFINOVANE PLATBY #MRIZKA# zpracovani BUY or SELL</summary>
/// <typeparam name="W"></typeparam>
public class MarketCoreDefinedSharpBroker<W> : MarketCore<W>, IMarketCoreSharpBroker<W>
    where W : ICryptoCurrency
{
    private readonly IBrokerMoneyProcessExtraDataService<W> _extraDataService;
    private readonly ILogger<MarketCoreDefinedSharpBroker<W>> _logger;
    private readonly IAppRobo _appRobo;
    private readonly IDefinedMoneyProcessMarket<W> _pm;

    private readonly ITransactionDataDriver<W> _transactionDataDriver;
    //  readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

    public MarketCoreDefinedSharpBroker(
        IDefinedMoneyProcessMarket<W> pm,
        ILogger<MarketCoreDefinedSharpBroker<W>> logger,
        ICoinMateRobo<W> coinMateRobo,
        ITransactionDataDriver<W> transactionDataDriver,
        IBrokerMoneyProcessExtraDataService<W> extraDataService,
        IAppRobo appRobo)
        : base(coinMateRobo, logger)
    {
        _pm = pm;
        _logger = logger;
        _appRobo = appRobo;
        _transactionDataDriver = transactionDataDriver;
        _extraDataService = extraDataService;
    }

    protected override IWallet BrokerWallet { get; set; } = default!;
    protected override string BrokerWalletName => nameof(MarketCoreDefinedSharpBroker<W>);

    public async Task ConnectToMarketAsync()
    {
        _pm.Init();
        var brokerWallet = _pm.GlobalWallet.GetWallet(BrokerWalletName); // aktualni penezenka pro tento process  . 
        if (brokerWallet is null)
        {
            _pm.GlobalWallet.SetWallet(BrokerWalletName, BrokerWallet = new Wallet(_pm.GlobalWallet.CryptoCurrency));
            SetBrokerWallet(BrokerWallet);
            await _pm.SaveWalletAsync();
        }
        else
        {
            BrokerWallet = brokerWallet;
            //TODO check jesti hodnoty nejsou nulove
            SetBrokerWallet(BrokerWallet);
        }

        await _transactionDataDriver.Load();
        await _cmr.InitRoboAsync((W)_pm.GlobalWallet.CryptoCurrency, _appRobo,
            _logger); // TODO OT: zmena na Market symbol (zjistit)
        var firstTicker = await _cmr.GetTickerAsync();
        var buyOrSellOrders = _pm.InicializationFirstSharpStrategy(firstTicker, brokerWallet!, _extraDataService);

        await BuyOrSell(buyOrSellOrders);
        //Inicializuj data z penezenky pri spusteni (nahazej data do marketu dle nejake definice mrizky)
    }

    public void SetBrokerWallet(IWallet brokerWallet)
    {
        _pm.SetBrokerWallet(BrokerWallet);
    }

    private DateTime questionToMarket = DateTime.Now;

    public async Task RunAsync()
    {
        try
        {
            // nacti aktualni order z Burzy
            var ticker = await IsTheSameTickerWithLastTickerAsync();
            if (ticker is null) return;

            //vypocti profit 
            var completedOrderInMarket = await _cmr.GetCompletedOrderDetailsAsync();
            var buyOrSellOrders = await _pm.RunProcessingAsync(ticker, _extraDataService, completedOrderInMarket.ToList());
            await BuyOrSell(buyOrSellOrders);
            var openOrder = await _cmr.GetOpenOrderDetailAsync();
            CheckOpenOrdersAndRemoveFromLocalOrders(openOrder);
        }
        catch (Exception ex)
        {
            throw new BussinesExceptions($"SHARP Strategy [{BrokerWallet.MarketSymbol}] have error.", ex);
        }
    }

    private void CheckOpenOrdersAndRemoveFromLocalOrders(IEnumerable<ExchangeOrderResult> openOrders)
    {
        foreach (var localOpenOrder in _extraDataService.GetOpenOrderTransaction().ToList())
        {
            if (openOrders.Any(x => x.OrderId == localOpenOrder.OrderResult.OrderId)) continue;
            _logger.LogInformation("Order: {OrderId}  is not found in open order section at online Market",
                localOpenOrder.OrderResult.OrderId);
            _extraDataService.RemoveTransaction(localOpenOrder);
        }
    }

    private async Task BuyOrSell(List<MarketProcessBuyOrSell> buyOrSellOrders)
    {
        if (!buyOrSellOrders.Any()) return;

        try
        {
            foreach (var buyOrSell in buyOrSellOrders)
            {
                // Nakup a zaloguj
                _logger.LogDebug(ObjectDumper.Dump(buyOrSell));

                // vytvor platbu (orderPlate)
                var orderRequest = _cmr.CreateExchangeOrderRequest(buyOrSell);
                var orderResult = await _cmr.PlaceOrderAsync(orderRequest);
                _logger.LogDebug("{Type} - Actual transaction {@OrderResult}", nameof(SharpProcessingMarket<W>), orderResult);

                var transaction =
                    _transactionDataDriver.Add(orderRequest, orderResult, _pm.GlobalWallet, buyOrSell, BrokerWalletName);
                _extraDataService.AddTransaction(transaction);
                Console.WriteLine();

                await RecalculateWalletAfterBuyOrSellOrderAsync(orderResult); // prepocti penezenku
            }
        }
        finally
        {
            // uloz do Csv souboru
            await _extraDataService.SaveDataAsync();
            await _transactionDataDriver.SaveAsync();
        }
    }

    private Task RecalculateWalletAfterBuyOrSellOrderAsync(ExchangeOrderResult orderResult)
    {
        if (orderResult.Result.IsCompleted())
        {
            CalculateActualTransactionIntoBrokerWallet(orderResult); // snizi a zvysi hodnotu
            _pm.GlobalWallet.SetWallet(BrokerWalletName, BrokerWallet);
            _pm.CalculateGlobalWallet();
            return _pm.SaveWalletAsync();
        }

        return Task.CompletedTask;
    }
}