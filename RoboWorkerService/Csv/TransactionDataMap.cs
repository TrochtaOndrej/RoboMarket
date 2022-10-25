using System.Globalization;
using CsvHelper.Configuration;
using RoboWorkerService.Market.Model;

namespace RoboWorkerService.Csv;

public class TransactionDataMap: ClassMap<TransactionData>
{
    public TransactionDataMap()
    {
        AutoMap(CultureInfo.InvariantCulture);
        Map(x => x.StrategyName);
        Map(x => x.BuyOrSell);
        Map(x => x.BuyOrSell);
       // Map(x => x.Wallet);
       // Map(x => x.OrderRequest);
        Map(x => x.OrderResult);
    }
}