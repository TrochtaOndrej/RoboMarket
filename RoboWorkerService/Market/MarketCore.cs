using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ExchangeSharp;
using RoboWorkerService.Interface;
using RoboWorkerService.Market.Enum;
using RoboWorkerService.Market.Model;
using RoboWorkerService.Market.Processing;
using RoboWorkerService.Robo;

namespace RoboWorkerService.Market
{
    public class MarketCore<T> where T : ICryptoCurrency
    {
        protected readonly ICoinMateRobo<T> _cmr;
        private readonly ILogger _logger;

        public MarketCore(ICoinMateRobo<T> cmr, ILogger logger)
        {
            _cmr = cmr;
            _logger = logger;
        }

        private ExchangeTicker _lastTicker = default!;

        protected async Task<ExchangeTicker?> IsTheSameTickerWithLastTickerAsync()
        {
            var ticker = await _cmr.GetTickerAsync();
            if (ticker.Ask == _lastTicker?.Ask && ticker.Bid == _lastTicker?.Bid)
            {
                Console.Write("-");
                return null;
            }
            //TODO OT LOW: smazat az bude GUI jinde
            Console.WriteLine();

            _logger.LogInformation("Market position BUY: {ask}, SELL {sell}", ticker.Ask, ticker.Bid);

            _lastTicker = ticker;
            return _lastTicker;
        }

        protected void CalculateActualTransactionIntoWallet(IWallet<T> wallet, ExchangeOrderResult exchange)
        {
            var fees = GetFeesFromOrderInBtc(exchange);
            if (exchange.Price is null) _logger.LogWarning("Price is null after BUY or SELL");
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
            wallet.CryptoPositionTransaction = exchange.Price ?? 0;


            _logger.LogInformation("Fees {fees} | Transaction {@transaction}", fees, exchange);
            _logger.LogInformation("Actual Wallet {wallet}", wallet.ToString());
        }

        protected decimal GetFeesFromOrderInBtc(ExchangeOrderResult exchange)
        {
            //exchange.Result == ExchangeAPIOrderResult.Filled
            var fees = exchange.Fees.HasValue ? exchange.Fees / 2 : 0;
            if (fees != 0)
            {
                return fees.Value / exchange.Price ?? 0;
            }

            return 0;
        }
    }

    public interface IMarketCoreCupBroker<W> where W : ICryptoCurrency
    {
        Task RunAsync();
        Task ConnectToMarketAsync();
    }

    /// <summary> Jedna se o Cup metodu zpracovani BUY or SELL</summary>
    /// <typeparam name="W"></typeparam>
    public class MarketCoreCupBroker<W> : MarketCore<W>, IMarketCoreCupBroker<W> where W : ICryptoCurrency
    {
        private readonly ICupProcessingMarket<W> _pm;
        private readonly ILogger<MarketCoreCupBroker<W>> _logger;
        readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        public MarketCoreCupBroker(
            ICupProcessingMarket<W> pm,
            ICoinMateRobo<W> cmr,
            ILogger<MarketCoreCupBroker<W>> logger) : base(cmr, logger)
        {
            _pm = pm;
            _logger = logger;
        }

        public async Task ConnectToMarketAsync()
        {
            _pm.Init();
            await _cmr.InitRoboAsync((W)_pm.Wallet.CryptoCurrency);
        }

        public Task RunAsync()
        {
            return CheckMarket();
        }

        private async Task CheckMarket()
        {
            await _semaphoreSlim.WaitAsync();
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
                var resultOrder = await _cmr.PlaceOrderAsync(buyOrSell);
                _logger.LogDebug("Actual transaction {@transaction}", resultOrder);
                // _pm.AddTransaction(resultOrder);
                CalculateActualTransactionIntoWallet(_pm.Wallet, resultOrder); // snizi a zvysi hodnotu
                 _pm.SaveWallet();
                _logger.LogDebug(ObjectDumper.Dump(resultOrder));

                Console.WriteLine();
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

    }


    //public class MarketCore
    //{
    //    public static void Init(IServiceCollection collection)
    //    {
    //        MarketCoreToServices<CupProcessingMarket>(CryptoDefinedList.BTC_EUR);
    //    }

    //    public static void MarketCoreToServices<T>(CryptoCurrency crypto ) where T : IProcessingMarketValue
    //    {
    //        var wallet = new Wallet<CryptoBTC>(new CryptoBTC());
    //      //  var walletTransaction = new trans

    //    }
    //    //init Market nacteni dat z json 9penezenka, transakce
    //    // Process zpracovani
    //    // Penezenka
    //    // Market currency
    //    // ulozeni dat do Json

    //    // Ulozeni poslednich transakci
    //    //  - Nakup, Prodej

    //    // Zjisteni transakci v Marketu, ktere se provedly
    //    // ukladani do databaze
    //}

    //public class MarketBroke
    //{

    //}

}
