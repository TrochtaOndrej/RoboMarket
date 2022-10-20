using System.Xml.Linq;
using RoboWorkerService.Market.Enum;
using RoboWorkerService.Market.Model;

namespace RoboWorkerService.Market.Processing.DefineMoney;

public record BrokerMoneyProcessExtraData
{
    /// <summary> Jedna se o data, ktere maji otevrene ordery a nebo se s ordery neco dale zamysli </summary>
    public List<TransactionData> TransactionData { get; set; } = new List<TransactionData>();

    /// <summary> Hodnoty pro strategii nakupu </summary>
    public MoneyProcessDataBuy ProcessDataBuy { get; set; } = new MoneyProcessDataBuy();

    /// <summary> Hodnoty pro strategii prodeje </summary>
    public MoneyProcessDataSell ProcessDataSell { get; set; } = new MoneyProcessDataSell();
}

public record MoneyProcessDataSell : SetMoneyProcessData
{
    /// <summary> Jedna se o cenu, ktera bude rozpocitana BTC,ETH,.. </summary>
    public decimal PriceInCrypto { get; set; } = 0m;

    public MoneyProcessDataSell()
    {
        MarketProcessType = MarketProcessType.Sell;
    }
}

public record MoneyProcessDataBuy : SetMoneyProcessData
{
    /// <summary> Jedna se o cenu, ktera bude rozpocitana </summary>
    public decimal PriceInEur { get; set; } = 100m;

    public MoneyProcessDataBuy()
    {
        MarketProcessType = MarketProcessType.Buy;
    }
}

public record SetMoneyProcessData
{
    public MarketProcessType MarketProcessType { get; set; } = MarketProcessType.None;
    /// <summary> dloni hranice nakupu v procentech </summary>
    public decimal PercentSpectrumStart { get; set; } = 0.7m;
    /// <summary> horni hranice nakupu </summary>
    public decimal PercentSpectrumEnd { get; set; } = 5.0m;
    /// <summary> percentialni krok nakupu </summary>
    public decimal PercentStepCalculatePrice { get; set; } = 0.1m;
}