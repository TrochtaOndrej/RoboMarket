using Helper.Serialization;
using RoboWorkerService.Config;
using RoboWorkerService.Market;
using RoboWorkerService.Market.Enum;
using RoboWorkerService.Market.Model;
using RoboWorkerService.Market.Processing;
using RoboWorkerService.Robo;

namespace RoboWorkerService;
public static class RoboServices
{
    public static IServiceCollection AddRoboServices(this IServiceCollection services)
    {
        services.AddSingleton<IJsonConvertor, JsonConvertor>();
        services.AddSingleton<IConfig, Config.Config>();
        services.AddSingleton<ICryptoBTC, CryptoBTC>();
        AddMarketBurker<ICryptoBTC>(services);

      //  AddMarketBurker<ICryptoETH>(services);

        services.AddSingleton(typeof(IWallet<>), typeof(Wallet<>));

        return services;
    }

    public static IServiceCollection AddMarketBurker<T>(this IServiceCollection services) where T : ICryptoCurrency
    {
        
        services.AddSingleton<IMarketCoreCupBroker<T>, MarketCoreCupBroker<T>>();
        services.AddSingleton<ICupProcessingMarket<T>, CupProcessingMarket<T>>();
        services.AddSingleton<ICoinMateRobo<T>, CoinMateRobo<T>>();
        
        return services;
    }
}
