﻿using ExchangeSharp;
using Newtonsoft.Json;
using RoboWorkerService.Interface;
using RoboWorkerService.Market.Enum;
using RoboWorkerService.Market.Model;
using System.Threading;

namespace RoboWorkerService.Robo;

public class MarketProcessing : IMarketProcessing
{
    private readonly ICoinMateRobo _coinMateRobo;
    private readonly IWalletMarketTransactionData _walletMarketTransactionData;
    private readonly ICalculateCrypto _calculateCrypto;
    private readonly ILogger<MarketProcessing> _logger;
    private readonly IActualMarketValue _actualMarketValue;
    private Wallet _wallet = default!;

    private ExchangeTicker _lastTicker = default!;

    public MarketProcessing(
        ICoinMateRobo coinMateRobo,
        IWalletMarketTransactionData walletMarketTransactionData,
        ICalculateCrypto calculateCrypto,
        ILogger<MarketProcessing> logger,
        IActualMarketValue actualMarketValue,
        Wallet wallet)
    {
        _coinMateRobo = coinMateRobo;
        _walletMarketTransactionData = walletMarketTransactionData;
        _calculateCrypto = calculateCrypto;
        _logger = logger;
        _actualMarketValue = actualMarketValue;
        _wallet = wallet;
    }

    public async Task InitAsync(MarketCurrencyType walletCurrency)
    {
        await _coinMateRobo.InitRoboAsync(_wallet.NameCurrency);
        var ticker = await _coinMateRobo.GetTickerAsync();

        //Nastaveni vychozi penezenky srovnane na aktualni hladinu EUR a BTC
        if (_wallet.CryptoAccountValue == 0)
        {
            const decimal eurInWallet = 400m;
            _wallet.CryptoAccountValue = eurInWallet / ((ticker.Ask + ticker.Bid) / 2); // 100 euro v Cryptu
            _wallet.EurAccountValue = eurInWallet;
        }
    }
    static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
    public async Task RunAsync()
    {
        await semaphoreSlim.WaitAsync();
        try
        {
           
            // nacti novy order z Burzy
            var ticker = await _coinMateRobo.GetTickerAsync();
            if (ticker.Ask == _lastTicker?.Ask && ticker.Bid == _lastTicker?.Bid)
            {
                Console.Write("-");
                return;
            }
            Console.WriteLine();
           _lastTicker = ticker;
            _logger.LogInformation("Market position BUY: {ask}, SELL {sell}", ticker.Ask, ticker.Bid);

            //vypocti profit 
            _actualMarketValue.SetActualValue(ticker);

            var buyOrSell = _calculateCrypto.CalculateSellOrBuy(1M, _actualMarketValue, 1);
            if (buyOrSell == null) return;

            _logger.LogDebug(ObjectDumper.Dump(buyOrSell));

            // vytvor platbu (orderPlate)
            var resultOrder = await _coinMateRobo.PlaceOrderAsync(buyOrSell);
            _logger.LogDebug("Actual transaction {@transaction}", resultOrder);
            _walletMarketTransactionData.AddTransaction(resultOrder);
            Wallet.SaveWalletToJsonFile(_wallet);
            CalculateActualTransactionIntoWallet(_wallet, resultOrder); // snizi a zvysi hodnotu
            _logger.LogDebug(ObjectDumper.Dump(resultOrder));
            Console.WriteLine();
        }
        finally
        {
            semaphoreSlim.Release();
        }

    }

    private decimal GetFeesFromOrderInBtc(ExchangeOrderResult exchange)
    {
        //exchange.Result == ExchangeAPIOrderResult.Filled
        var fees = exchange.Fees.HasValue ? exchange.Fees / 2 : 0;
        if (fees != 0)
        {
            return fees.Value / exchange.Price ?? 0;
        }

        return 0;
    }

    private void CalculateActualTransactionIntoWallet(Wallet wallet, ExchangeOrderResult exchange)
    {
        var fees = GetFeesFromOrderInBtc(exchange);

        if (exchange.IsBuy)
        {
            wallet.CryptoAccountValue += exchange.Amount - fees;
            wallet.EurAccountValue -= (exchange.Amount - fees) * exchange.Price ?? 0;
        }
        else
        {
            wallet.CryptoAccountValue -= exchange.Amount - fees;
            wallet.EurAccountValue += (exchange.Amount - fees) * exchange.Price ?? 0;
        }

        _logger.LogInformation("Fees {fees} | Transaction {@transaction}", fees, exchange);
        _logger.LogInformation("Actual Wallet {wallet}", wallet.ToString());
    }

}

