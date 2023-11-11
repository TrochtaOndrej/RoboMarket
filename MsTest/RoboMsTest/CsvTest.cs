using System.Configuration;
using ExchangeSharp;
using Helper.Interface;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RoboWorkerService;
using RoboWorkerService.Csv;
using RoboWorkerService.Interface;
using RoboWorkerService.Interfaces;
using RoboWorkerService.Json;
using RoboWorkerService.Market.Enum;
using RoboWorkerService.Market.Processing;

namespace RoboMsTest;

[TestClass]
public class CsvTest : BaseTest
{
    public CsvTest() : base()
    {
    }

    public override void InitService(ServiceCollection service)
    {
        service.AddSingleton(typeof(IMarketTransactionCsv<>), typeof(MarketTransactionCsv<>));
        service.AddJsonService();
    }

    [TestMethod]
    public Task ExportToCsv_ExchangeOrderResult()
    {
        var csv = ServiceProvider.GetRequiredService<IMarketTransactionCsv<ExchangeOrderResult>>();
        csv.DeleteCsvFile();
        csv.WriteToFileCsv(new ExchangeOrderResult());
        csv.WriteToFileCsv(new ExchangeOrderResult()
        {
            Amount = 5,
            Fees = 0.1m,
            Message = "MsTest",
            IsBuy = true,
            Result = ExchangeAPIOrderResult.Filled,
            OrderId = "order-id-1",
            TradeDate = new DateTime(2022, 12, 24)
        });

        return Verify(File.ReadAllText(csv.FileNameCsv), VerifySettingsTest);
    }
}

[TestClass]
public class ProcessingCupTest : BaseTest
{
    public ProcessingCupTest()
    {

    }

    public override void InitService(ServiceCollection service)
    {
        RoboServices.AddRoboServices(service);
        RoboServices.AddJsonService(service);

    }
}