using ExchangeSharp;
using RoboWorkerService.Interfaces;

namespace RoboWorkerService.Market.Model;

public record TransactionData
{
    public ExchangeOrderRequest OrderRequest { get; set; }
    public ExchangeOrderResult OrderResult { get; set; }
    public IWallet Wallet { get; set; }
    public MarketProcessBuyOrSell BuyOrSell { get; set; }

    public string ProcessingType { get; set; }
}