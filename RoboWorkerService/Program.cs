using RoboWorkerService;
using RoboWorkerService.Market.Enum;
using RoboWorkerService.Market.Model;
using Serilog.Events;
using Serilog;

AppDomain.CurrentDomain.ProcessExit += new EventHandler(HostApp.CurrentDomain_ProcessExit);
//Serilog
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

// app
HostApp.Host = Host.CreateDefaultBuilder(args)
   .UseSerilog(Log.Logger)
   .ConfigureServices(services =>
   {
       services.AddHostedService<Worker>();
       services.AddJsonService();
       services.AddRoboServices();
   })
   .Build();

try
{
    await HostApp.Host.RunAsync();
}
catch (Exception ex)
{
    Log.Logger.Write(LogEventLevel.Fatal, ex.ToString());
}

public static class HostApp
{
    public static IHost Host { get; set; }

    public static void CurrentDomain_ProcessExit(object sender, EventArgs e)
    {
       // var wallet = HostApp.Host.Services?.GetService<Wallet>();
       //if (wallet!= null) Wallet.SaveWalletToJsonFile(wallet);
    }
}

