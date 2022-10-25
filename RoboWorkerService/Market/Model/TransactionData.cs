using CsvHelper.Configuration;
using ExchangeSharp;
using RoboWorkerService.Csv;
using RoboWorkerService.Interfaces;

namespace RoboWorkerService.Market.Model;
public record TransactionData 
{
    public ExchangeOrderRequest OrderRequest { get; set; }
    public ExchangeOrderResult OrderResult { get; set; }
    public IWallet Wallet { get; set; }
    public MarketProcessBuyOrSell BuyOrSell { get; set; }

    public string StrategyName { get; set; }
}