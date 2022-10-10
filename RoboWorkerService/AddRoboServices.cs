using Newtonsoft.Json;
using NLog.Fluent;
using RoboWorkerService.Interface;
using RoboWorkerService.Market.Business;
using RoboWorkerService.Market.Enum;
using RoboWorkerService.Market.Model;
using RoboWorkerService.Robo;
using Log = Serilog.Log;

namespace RoboWorkerService;
public static class RoboServices
{
    public static IServiceCollection AddRoboServices(this IServiceCollection services)
    {
        services.AddSingleton<Wallet>( s =>
        {
            try
            {
                if (File.Exists("wallet.json"))
                {
                    using (StreamReader reader = new StreamReader("wallet.json"))
                    {
                        string json = reader.ReadToEnd();
                        return JsonConvert.DeserializeObject<Wallet>(json) ?? new Wallet(CryptoCurrencyDefinitionList.BTC_EUR);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex,"Load Wallet failed!");
            }

            return new Wallet(CryptoCurrencyDefinitionList.BTC_EUR);
        });

        services.AddSingleton<ICoinMateRobo, CoinMateRobo>();
        services.AddSingleton<IMarketProcessing, MarketProcessing>();
        services.AddSingleton<ICupProcessMarketValue, CupProcessMarket>();
        services.AddSingleton<ICalculateCrypto, CalculateCrypto>();
        services.AddSingleton<IWalletMarketTransactionData, WalletMarketTransactionData>();

        return services;
    }
}
