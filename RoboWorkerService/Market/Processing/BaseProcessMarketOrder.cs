using ExchangeSharp;
using Helper.Interface;
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
    private readonly IAppRobo _appRobo;
    private readonly T _cryptoCurrency;
    private readonly string _processingName;
    protected string FileName;
    private IWallet _brokerWallet;
    public IWallet<T> GlobalWallet { get; protected set; }

    public IWallet BrokerWallet => _brokerWallet ?? GlobalWallet;

    public BaseProcessMarketOrder(
        T cryptoCurrency,
        ILogger logger,
        IWallet<T> globalWallet,
        IConfig config,
        IJsonConvertor json,
        IAppRobo appRobo,
        string processingName // nazev parent tridy pro rozliseni dat
    ) : base()
    {
        Config = config;
        _json = json;
        _appRobo = appRobo;
        _cryptoCurrency = cryptoCurrency;
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
            throw new BussinesExceptions(
                $" CryptoCurrency [{CryptoCurrency.ToString()}] is no the same with order market currency [{ticker.MarketSymbol}] !");
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

    protected void Validation()
    {
        if (!BrokerWallet.CryptoCurrency.Crypto.ToString()
                .Equals(CryptoCurrency.Crypto.ToString(), StringComparison.InvariantCultureIgnoreCase))
            throw new BussinesExceptions(" GlobalWallet marketCurrency is no the same with MarketValue!");
    }

    public MarketProcessBuyOrSell? CreateBuyOrderEur(decimal defineProfitInPercently, decimal investMoneyEur,
        MarketProcessType marketProcessType, decimal? cryptoPosition = null)
    {
        if (defineProfitInPercently < 0.6m)
            throw new BussinesExceptions("Profit must by more then 0.6%. This percent is for market feeds");

        if (BrokerWallet.CryptoPositionTransaction < 100)
            throw new BussinesExceptions(
                $"Actual BrokerWallet.CryptoPositionTransaction is less 100. PLease fill the position in wallet. {FileName}");

        // pokud neni definovana pozice tak , se bere aktualni pozice v marketu jinak se nastavi hodnota, kterou
        // definujeme v promene. Tahle podminka jen nastavuje pro vypocet hodnotu predanou
        if (cryptoPosition is not null)
        {
            CryptoPriceBuy = CryptoPriceSell = cryptoPosition.Value;
        }

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
                InternalData = new InternalDataBuyOrSell()
                {
                    Strategy = _processingName,
                    StartingPointPositionToBuyOrSell = CryptoPriceBuy,
                    InternalNumber = _appRobo.RoboConfig.Data.GetNumberOrder()
                },
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
                    InternalData = new InternalDataBuyOrSell()
                    {
                        Strategy = _processingName,
                        StartingPointPositionToBuyOrSell = CryptoPriceSell,
                        InternalNumber = _appRobo.RoboConfig.Data.GetNumberOrder()
                    }
                };
            }
        }

        return null;
    }

    #region JSon

    public virtual void Init()
    {
        if (File.Exists(FileName))
        {
            var str = File.ReadAllText(FileName);
            var www = _json.ToInstance<Wallet<T>>(str);
            // if (www.MarketSymbol != GlobalWallet.MarketSymbol)
            // throw new BussinesExceptions(
            // $"Config for GlobalWallet is broken. The MarketSymbol is different. [{www.MarketSymbol}]!=[{GlobalWallet.MarketSymbol}] ");
            GlobalWallet.CryptoAccountValue = www.CryptoAccountValue;
            GlobalWallet.CryptoPositionTransaction = www.CryptoPositionTransaction;
            GlobalWallet.EurAccountValue = www.EurAccountValue;
            GlobalWallet.CryptoBrokerWallet = www.CryptoBrokerWallet;
        }
        else
        {
            var str = _json.ToJson<IWallet<T>>(GlobalWallet);
            File.WriteAllText(FileName, str);
            throw new BussinesExceptions($"Is created new config file for GlobalWallet in [{FileName}] ");
        }
    }

    protected Task SaveWalletToFileAsync()
    {
        return _json.ToFileJsonAsync(FileName, GlobalWallet, CancellationToken.None);
    }

    #endregion
}