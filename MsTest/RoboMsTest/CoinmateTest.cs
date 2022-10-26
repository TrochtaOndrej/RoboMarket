using System.Configuration;
using CryptoBotCore.API;
using ExchangeSharp;
using Helper.Interface;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RoboWorkerService;
using RoboWorkerService.Csv;
using RoboWorkerService.Interfaces;
using RoboWorkerService.Json;

namespace RoboMsTest;

[TestClass]
public class CoinmateTestMarket : BaseTest
{
    public CoinmateTestMarket() : base()
    {
    }

    public override void InitService(ServiceCollection service)
    {
        service.AddSingleton(typeof(IMarketTransactionCsv<>), typeof(MarketTransactionCsv<>));
        service.AddJsonService();
        service.AddSingleton<ICoinmateAPI, CoinmateAPI>();
    }

    [TestMethod]
    public async Task ExportToCsv_ExchangeOrderResult()
    {
        var coinmateApi = ServiceProvider.GetRequiredService<ICoinmateAPI>();
        await coinmateApi.buyOrderAsync(10m, 14500, "BTC_EUR");
    }
}