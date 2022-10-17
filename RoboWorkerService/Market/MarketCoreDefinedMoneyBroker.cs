using ExchangeSharp;
using RoboWorkerService.Interfaces;
using RoboWorkerService.Market.Enum;
using RoboWorkerService.Market.Model;
using RoboWorkerService.Market.Processing;
using RoboWorkerService.Robo;

namespace RoboWorkerService.Market;

/// <summary> Jedna se o UZAVIREJ PREDEM DEFINOVANE PLATBY #MRIZKA# zpracovani BUY or SELL</summary>
/// <typeparam name="W"></typeparam>
public class MarketCoreDefinedMoneyBroker<W> : MarketCore<W>, IMarketCoreDefinedMoneyBroker<W> where W : ICryptoCurrency
{
    private readonly ILogger<MarketCoreDefinedMoneyBroker<W>> _logger;
    private readonly IDefinedMoneyProcessMarket<W> _pm;

    private readonly ITransactionProcessing<W> _transaction;

    private readonly IBrokerMoneyProcessExtraDataService<W> _extraDataService;
    //  readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

    public MarketCoreDefinedMoneyBroker(
        IDefinedMoneyProcessMarket<W> pm,
        ILogger<MarketCoreDefinedMoneyBroker<W>> logger,
        ICoinMateRobo<W> coinMateRobo,
        ITransactionProcessing<W> transaction,
        IBrokerMoneyProcessExtraDataService<W> extraDataService)
        : base(coinMateRobo, logger)
    {
        _pm = pm;
        _logger = logger;
        _transaction = transaction;
        _extraDataService = extraDataService;
    }

    protected override IWallet BrokerWallet { get; set; }
    protected override string BrokerWalletName => nameof(MarketCoreDefinedMoneyBroker<W>);

    public async Task ConnectToMarketAsync()
    {
        _pm.Init();

        var brokerWallet = _pm.GlobalWallet.GetWallet(BrokerWalletName); // aktualni penezenka pro tento process  . 
        if (brokerWallet == null)
        {
            _pm.GlobalWallet.SetWallet(BrokerWalletName, BrokerWallet = new Wallet(_pm.GlobalWallet.CryptoCurrency));
            SetBrokerWallet(BrokerWallet);
            _pm.SaveWallet();
        }
        else
        {
            BrokerWallet = brokerWallet;
            //TODO check jesti hodnoty nejsou nulove
            SetBrokerWallet(BrokerWallet);
        }

        await _transaction.Load();
        await _cmr.InitRoboAsync((W)_pm.GlobalWallet.CryptoCurrency); // TODO OT: zmena na Market symbol (zjistit)
    }

    public void SetBrokerWallet(IWallet brokerWallet)
    {
        _pm.SetBrokerWallet(BrokerWallet);
    }

    public Task RunAsync()
    {
        return CheckMarket();
    }

    /// <summary> Overi jestli jsou nejake otevrene transakce a ty zkontroluje na Market </summary>
    private void CheckOrdersByTransaction(ITransactionProcessing<W> tp, IDefinedMoneyProcessMarket<W> pm)
    {
        var openTransaction = tp.GetTransaction(x =>
            x.OrderResult.IsBuy && x.OrderResult.Result == ExchangeAPIOrderResult.Open ||
            x.OrderResult.Result == ExchangeAPIOrderResult.PendingOpen).ToList();

        if (!openTransaction.Any())
        {
            _logger.LogInformation("No open transaction in {Crypto}", _pm.Crypto);
            return;
        }

        var data = _cmr.GetOpenOrderDetailsAsync().Result.ToList();

        foreach (var transactionData in openTransaction)
        {
            var findTransaction = data.FirstOrDefault(x =>
                x.TradeId == transactionData.OrderResult.TradeId && x.OrderId == transactionData.OrderResult.OrderId);
            if (findTransaction == null) continue;

            if (transactionData.OrderResult.Result != findTransaction.Result)
            {
                // doslo ke zmene transakce (vykonal se nakup, nebo se zrusil nakup a pod.
            }
        }
    }

    private async Task CheckMarket()
    {
        //   await _semaphoreSlim.WaitAsync();
        // nacti novy order z Burzy
        var ticker = await IsTheSameTickerWithLastTickerAsync();
        if (ticker is null) return;

        CheckOrdersByTransaction(_transaction, _pm);

        //vypocti profit 
        var buyOrSell = _pm.RunProcessing(ticker);
        if (buyOrSell == null) return; // zadny profit 


        _logger.LogDebug(ObjectDumper.Dump(buyOrSell));

        // vytvor platbu (orderPlate)
        var orderRequest = _cmr.CreateExchangeOrderRequest(buyOrSell);
        var orderResult = await _cmr.PlaceOrderAsync(orderRequest);
        _logger.LogDebug("Actual transaction {Transaction}", orderResult);
        // _pm.AddTransaction(resultOrder);
        CalculateActualTransactionIntoBrokerWallet(orderResult); // snizi a zvysi hodnotu
        _pm.CalculateGlobalWallet();
        _pm.SaveWallet();
        var transaction = _transaction.Add(orderRequest, orderResult, _pm.GlobalWallet, buyOrSell, typeof(W));
        _extraDataService.AddTransaction(transaction);
        await _transaction.SaveAsync();
        await _extraDataService.SaveData();
        _logger.LogDebug(ObjectDumper.Dump(orderResult));

        Console.WriteLine();
    }
}