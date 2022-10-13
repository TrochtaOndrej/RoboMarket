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
    public IWallet<T> Wallet { get; protected set; }
    

    public BaseProcessMarketOrder(
        ILogger logger,
        IWallet<T> wallet,
        IConfig config,
        IJsonConvertor json,
        string processingName // nazev parent tridy pro rozliseni dat
        ) : base()
    {
        Config = config;
        _json = json;
        _processingName = processingName;
        Wallet = wallet;
        FileName = Config.ConfigPath + Wallet.MarketSymbol + "_" + processingName + ".json";
    }

    /// <summary> Aktualni hodnota v bitcoinu v penezence prevedena na EUR pro Nakup</summary>
    public decimal GetWallet_EurToCryptoBuy()
    {
        Validation();
        return Wallet.CryptoAccountValue * CryptoPriceBuy;
    }

    /// <summary> Aktualni hodnota v bitcoinu v penezence prevedena na EUR pro Prodej</summary>
    public decimal GetWallet_EurToCryptoSell()
    {
        Validation();
        return Wallet.CryptoAccountValue * CryptoPriceSell;
    }

    public decimal GetWallet_EurToCrypto()
    {
        Validation();
        return (GetWallet_EurToCryptoBuy() + GetWallet_EurToCryptoSell()) / 2;
    }

    public void SetActualValueFromMarket(ExchangeTicker ticker)
    {
        if (!string.Equals(Wallet.MarketSymbol, ticker.MarketSymbol, StringComparison.InvariantCultureIgnoreCase))
            throw new BussinesExceptions($" CryptoCurrency [{CryptoCurrency.ToString()}] is no the same with order market currency [{ticker.MarketSymbol}] !");
        CryptoCurrency = new CryptoCurrency(ticker.MarketSymbol);
        CryptoPriceSell = ticker.Bid;
        CryptoPriceBuy = ticker.Ask;
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
        if (GetWallet_EurToCrypto() - Wallet.EurAccountValue >= 0) return MarketProcessType.Sell;
        return MarketProcessType.Buy;
    }

    protected void Validation()
    {
        if (!Wallet.CryptoCurrency.Crypto.ToString().Equals(CryptoCurrency.Crypto.ToString(), StringComparison.InvariantCultureIgnoreCase)) throw new BussinesExceptions(" Wallet marketCurrency is no the same with MarketValue!");
    }

    #region JSon
    public virtual void Init()
    {

        if (File.Exists(FileName))
        {
            var str = File.ReadAllText(FileName);
            var www = _json.ToInstance<Wallet<T>>(str);
            if (www.MarketSymbol != Wallet.MarketSymbol)
                throw new BussinesExceptions(
                    $"Config for Wallet is broken. The MarketSymbol is different. [{www.MarketSymbol}]!=[{Wallet.MarketSymbol}] ");

            Wallet.CryptoAccountValue = www.CryptoAccountValue;
            Wallet.CryptoPositionTransaction = www.CryptoPositionTransaction;
            Wallet.EurAccountValue = www.EurAccountValue;

        }
        else
        {
            var str = _json.ToJson<IWallet<T>>(Wallet);
            File.WriteAllText(FileName, str);
            throw new BussinesExceptions($"Is created new config file for Wallet in [{FileName}] ");
        }
    }

    protected void SaveWalletToFile()
    {
        var str = _json.ToJson<IWallet<T>>(Wallet);
        File.WriteAllText(FileName, str);
    }
    #endregion
}