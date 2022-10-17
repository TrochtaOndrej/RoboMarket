using ExchangeSharp;
using Helper.Serialization;
using RoboWorkerService.Config;
using RoboWorkerService.Interfaces;
using RoboWorkerService.Market.Enum;
using RoboWorkerService.Market.Model;

namespace RoboWorkerService.Market.Processing;

/// <summary> Bazova trida pro pomocne funkce pri vypoctech</summary>
/// <typeparam name="T"></typeparam>
public class BaseProcessMarketOrder<T> : MarketCrypto, IBaseProcessMarketOrder<T> 
    //where P : IProcessingMarketValue
    where T : ICryptoCurrency
{
    protected readonly IConfig Config;
    private readonly IJsonConvertor _json;
    private readonly string _processingName;
    protected string FileName;
    private IWallet _brokerWallet;
    public IWallet<T> GlobalWallet { get; protected set; }

    public IWallet BrokerWallet => _brokerWallet ?? GlobalWallet;

    public BaseProcessMarketOrder(
        ILogger logger,
        IWallet<T> globalWallet,
        IConfig config,
        IJsonConvertor json,
        string processingName // nazev parent tridy pro rozliseni dat
        ) : base()
    {
        Config = config;
        _json = json;
        _processingName = processingName;
        GlobalWallet = globalWallet;
        FileName = Config.ConfigPath + GlobalWallet.MarketSymbol + "_GlobalWallet.json";
    }

    public void SetBrokerWallet(IWallet brokerWallet)
    {
        _brokerWallet = brokerWallet;
    }

    /// <summary> Aktualni hodnota v bitcoinu v penezence prevedena na EUR pro Nakup</summary>
    public decimal GetWallet_EurToCryptoBuy()
    {
        Validation();
        return BrokerWallet.CryptoAccountValue * CryptoPriceBuy;
    }

    /// <summary> Aktualni hodnota v bitcoinu v penezence prevedena na EUR pro Prodej</summary>
    public decimal GetWallet_EurToCryptoSell()
    {
        Validation();
        return BrokerWallet.CryptoAccountValue * CryptoPriceSell;
    }

    public decimal GetWallet_EurToCrypto()
    {
        Validation();
        return (GetWallet_EurToCryptoBuy() + GetWallet_EurToCryptoSell()) / 2;
    }

    public void SetActualValueFromMarket(ExchangeTicker ticker)
    {
        if (!string.Equals(GlobalWallet.MarketSymbol, ticker.MarketSymbol, StringComparison.InvariantCultureIgnoreCase))
            throw new BussinesExceptions($" CryptoCurrency [{CryptoCurrency.ToString()}] is no the same with order market currency [{ticker.MarketSymbol}] !");
        CryptoCurrency = new CryptoCurrency(ticker.MarketSymbol);
        CryptoPriceSell = ticker.Bid;
        CryptoPriceBuy = ticker.Ask;
        BrokerWallet.CryptoCurrency = CryptoCurrency;
    }
    /// <summary> Investovane penize za nakup se vypocita fees </summary>
    /// <param name="investingValue"></param>
    /// <returns></returns>
    protected decimal CalculateFees(decimal investingValue)
    {
        decimal feesPercentlyInMarket = 0.6m; // Coinbase ma 0.6%
        return (investingValue / 100) * feesPercentlyInMarket;
    }

    public MarketProcessType GetActualMArketProcess()
    {
        // Penize na pozici - penezenka >=0 
        if (GetWallet_EurToCrypto() - BrokerWallet.EurAccountValue >= 0) return MarketProcessType.Sell;
        return MarketProcessType.Buy;
    }

    protected void Validation()
    {
        if (!BrokerWallet.CryptoCurrency.Crypto.ToString().Equals(CryptoCurrency.Crypto.ToString(), StringComparison.InvariantCultureIgnoreCase)) throw new BussinesExceptions(" GlobalWallet marketCurrency is no the same with MarketValue!");
    }

    #region JSon
    public virtual void Init()
    {

        if (File.Exists(FileName))
        {
            var str = File.ReadAllText(FileName);
            var www = _json.ToInstance<Wallet<T>>(str);
            if (www.MarketSymbol != GlobalWallet.MarketSymbol)
                throw new BussinesExceptions(
                    $"Config for GlobalWallet is broken. The MarketSymbol is different. [{www.MarketSymbol}]!=[{GlobalWallet.MarketSymbol}] ");

            GlobalWallet.CryptoAccountValue = www.CryptoAccountValue;
            GlobalWallet.CryptoPositionTransaction = www.CryptoPositionTransaction;
            GlobalWallet.EurAccountValue = www.EurAccountValue;
            GlobalWallet.ProcessingWallet = www.ProcessingWallet;
        }
        else
        {
            var str = _json.ToJson<IWallet<T>>(GlobalWallet);
            File.WriteAllText(FileName, str);
            throw new BussinesExceptions($"Is created new config file for GlobalWallet in [{FileName}] ");
        }
    }

    protected void SaveWalletToFile()
    {
        var str = _json.ToJson<IWallet<T>>(GlobalWallet);
        File.WriteAllText(FileName, str);
    }
    #endregion
}