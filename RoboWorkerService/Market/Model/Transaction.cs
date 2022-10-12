using ExchangeSharp;

namespace RoboWorkerService.Market.Model;

public class Transaction
{
    public List<ExchangeOrderResult> Trasactions = new List<ExchangeOrderResult>();

    public void Add(ExchangeOrderResult or)
    {
        Trasactions.Add(or);
    }
}