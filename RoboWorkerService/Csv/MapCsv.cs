using System.Globalization;
using ExchangeSharp;

namespace RoboWorkerService.Csv;

public class ExchangeOrderRequestCsvMap:ExchangeOrderRequest
{
    public FooMap()
    {
        AutoMap(CultureInfo.InvariantCulture);
        Map(m => m.Name).Name("The Name");
    }
}