﻿using RoboWorkerService.Interfaces;
using RoboWorkerService.Market.Enum;
using RoboWorkerService.Market.Model;
using RoboWorkerService.Market.Processing;
using RoboWorkerService.Robo;

namespace RoboWorkerService.Market;

/// <summary> Jedna se o Cup metodu zpracovani BUY or SELL</summary>
/// <typeparam name="W"></typeparam>
public class MarketCoreCupBroker<W> : MarketCore<W>, IMarketCoreCupBroker<W> where W : ICryptoCurrency
{
    private readonly ICupProcessingMarket<W> _pm;
    private readonly ILogger<MarketCoreCupBroker<W>> _logger;
    private readonly ITransactionProcessing<W> _transaction;

    private readonly string _walletName = nameof(MarketCoreCupBroker<W>);

    public IWallet BrokerWallet { get; private set; }
    //  readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

    public MarketCoreCupBroker(
        ICupProcessingMarket<W> pm,
        ICoinMateRobo<W> cmr,
        ILogger<MarketCoreCupBroker<W>> logger,
        ITransactionProcessing<W> transactionProcessing
        ) : base(cmr, logger)
    {
        _pm = pm;
        _logger = logger;
        _transaction = transactionProcessing;
    }

    public async Task ConnectToMarketAsync()
    {
        _pm.Init();
        var brokerWallet = _pm.Wallet.GetWallet(_walletName); // aktualni penezenka pro tento process
        if (brokerWallet == null) _pm.Wallet.SetWallet(_walletName, BrokerWallet = new Wallet(_pm.Wallet.CryptoCurrency));

        await _transaction.Load();
        await _cmr.InitRoboAsync((W)_pm.Wallet.CryptoCurrency);
    }

    public Task RunAsync()
    {
        return CheckMarket();
    }

    private async Task CheckMarket()
    {
        //   await _semaphoreSlim.WaitAsync();
        try
        {
            // nacti novy order z Burzy
            var ticker = await IsTheSameTickerWithLastTickerAsync();
            if (ticker is null) return;

            //vypocti profit 
            var buyOrSell = _pm.RunProcessing(ticker);
            if (buyOrSell == null) return; // zadny profit 

            _logger.LogDebug(ObjectDumper.Dump(buyOrSell));

            // vytvor platbu (orderPlate)
            var orderRequest = _cmr.CreateExchangeOrderRequest(buyOrSell);
            var orderResult = await _cmr.PlaceOrderAsync(orderRequest);
            _logger.LogDebug("Actual transaction {@transaction}", orderResult);
            // _pm.AddTransaction(resultOrder);
            CalculateActualTransactionIntoWallet(BrokerWallet, orderResult); // snizi a zvysi hodnotu
            _pm.SaveWallet();
            _transaction.Add(orderRequest, orderResult, _pm.Wallet, buyOrSell, typeof(W));
            await _transaction.SaveAsync();
            _logger.LogDebug(ObjectDumper.Dump(orderResult));

            Console.WriteLine();
        }
        finally
        {
            //     _semaphoreSlim.Release();
        }
    }

}