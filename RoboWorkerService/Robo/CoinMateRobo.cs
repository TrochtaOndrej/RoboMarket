using System.Security;
using ExchangeSharp;
using RoboWorkerService.Config;
using RoboWorkerService.Csv;
using RoboWorkerService.Interfaces;
using RoboWorkerService.Market.Enum;
using RoboWorkerService.Market.Model;

namespace RoboWorkerService.Robo;

public class CoinMateRobo<T> : ICoinMateRobo<T> where T : ICryptoCurrency
{
    private IExchangeAPI _iexApi = default!;
    private string _marketSymbol;
    private IAppRobo _appRobo;
    private IMarketTransactionCsv<ExchangeOrderRequest> _csvOrderRequestFile;
    private IMarketTransactionCsv<ExchangeOrderResult> _csvOrderResultFile;
    private ILogger _logger;

    public async Task InitRoboAsync(T symbol, IAppRobo appRobo, ILogger logger)
    {
        _logger = logger;
        _appRobo = appRobo;
        _csvOrderRequestFile = _appRobo.GetService<IMarketTransactionCsv<ExchangeOrderRequest>>();
        _csvOrderResultFile = _appRobo.GetService<IMarketTransactionCsv<ExchangeOrderResult>>();
        var configFile = appRobo.Config.RootPath + @$"\{appRobo.Config.DefineMarketAsType.Name}.bin";
        if (!File.Exists(configFile)) throw new FileNotFoundException("Config file not found. Path: " + configFile);

        _iexApi = await ExchangeAPI.GetExchangeAPIAsync(appRobo.Config.DefineMarketAsType);

        _iexApi.LoadAPIKeys(configFile);
        _iexApi.Passphrase = new SecureString();
        _iexApi.Cache = new MemoryCache(299);
        _iexApi.MethodCachePolicy["GetTickerAsync"] = TimeSpan.FromMilliseconds(300); //TODO OT: dat do configu delay a chache

        ExchangeAPI.UseDefaultMethodCachePolicy = false; // TODO: prozkoumat jaky to ma vliv na GetTickerAsync

        _marketSymbol = symbol.Crypto;

        foreach (char c in appRobo.Configuration.GetValue<string>("RoboApp:Passphrase")) _iexApi.Passphrase.AppendChar(c);
    }

    public Task<IEnumerable<ExchangeOrderResult>> GetValueAsync()
    {
        return _iexApi.GetOpenOrderDetailsAsync(_marketSymbol);
    }

    public Task<string> ExchangeRateBtcAsync()
    {
        return _iexApi.GlobalMarketSymbolToExchangeMarketSymbolAsync(_marketSymbol);
    }

    /// <summary> Aktualni hodnota na burze </summary>
    public Task<ExchangeTicker> GetTickerAsync()
    {
        return _iexApi.GetTickerAsync(_marketSymbol);
    }

    public Task<IEnumerable<ExchangeOrderResult>> GetCompletedOrderDetailsAsync()
    {
        return _iexApi.GetCompletedOrderDetailsAsync(_marketSymbol,
            DateTime.Now.Date.AddDays(-3)); //TODO OT: doresit datum od kdy si vyhledat uzavrene ordery
    }

    public async Task<ExchangeMarginPositionResult> MarginPositionResult()
    {
        var margin = await _iexApi.GetOpenPositionAsync(_marketSymbol);
        return margin;
    }

    public async Task<IEnumerable<ExchangeOrderResult>> GetOpenOrderDetailAsync()
    {
        var margin = await _iexApi.GetOpenOrderDetailsAsync(_marketSymbol);
        return margin;
    }

    public ExchangeOrderRequest CreateExchangeOrderRequest(MarketProcessBuyOrSell marketProcessBuyOrSell)
    {
        return new ExchangeOrderRequest
        {
            OrderType = OrderType.Limit,
            IsBuy = marketProcessBuyOrSell.ProcessType == MarketProcessType.Buy,
            MarketSymbol = marketProcessBuyOrSell.MarketSymbol,
            Price = marketProcessBuyOrSell.Price,
            Amount = marketProcessBuyOrSell.CryptoValue
            // IsPostOnly = marketProcessBuyOrSell.IsPostOnly
        };
    }

    /// <summary>  Buy or Sell on market </summary>
    public async Task<ExchangeOrderResult> PlaceOrderAsync(ExchangeOrderRequest orderRequest)
    {
        #region FakeData in Development

        if (_appRobo.Config.IsDevelopment) // pokud se jedna o development verzi - vraci FAKE DATA
            return new ExchangeOrderResult()
            {
                Amount = orderRequest.Amount,
                Fees = 0.01m,
                Message = "Development version",
                AmountFilled = orderRequest.Amount,
                Price = orderRequest.Price,
                IsBuy = orderRequest.IsBuy,
                OrderId = DateTime.Now.ToLongTimeString(),
                Result = ExchangeAPIOrderResult.Filled
            };

        #endregion

        orderRequest.ExtraParameters.Add("RoboNumberOrder", _appRobo.RoboConfig.Data.GetNumberOrder());
        var result = await _iexApi.PlaceOrderAsync(orderRequest);

        _csvOrderRequestFile.WriteToFileCsv(orderRequest);
        _csvOrderResultFile.WriteToFileCsv(result);
        return result;
    }
}