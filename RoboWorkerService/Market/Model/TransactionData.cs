using CsvHelper.Configuration;
using ExchangeSharp;
using RoboWorkerService.Csv;
using RoboWorkerService.Interfaces;

namespace RoboWorkerService.Market.Model;

public record TransactionData
{
    public ExchangeOrderRequest OrderRequest { get; set; } = null!;
    public ExchangeOrderResult OrderResult { get; set; } = null!;
    public IWallet Wallet { get; set; } = null!;
    public MarketProcessBuyOrSell BuyOrSell { get; set; } = null!;

    public string StrategyName { get; set; } = null!;
}