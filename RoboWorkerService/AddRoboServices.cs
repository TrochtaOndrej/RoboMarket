using Helper.Serialization;
using Newtonsoft.Json;
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

        services.AddSingleton<IConfig, Config.Config>();
       // CRYPTO CURRENCY
        services.AddSingleton<ICryptoBTC, CryptoBTC>();
        AddMarketBurker<ICryptoBTC>(services);

        services.AddSingleton<ICryptoALGO, CryptoALGO>();
        AddMarketBurker<ICryptoALGO>(services);

        services.AddSingleton<ICryptoETH, CryptoETH>();
        AddMarketBurker<ICryptoETH>(services);

        services.AddSingleton<ICryptoDOGE, CryptoDOGE>();
        AddMarketBurker<ICryptoDOGE>(services);

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

    public static IServiceCollection AddJsonService(this IServiceCollection services)
    {
        services.AddSingleton<IJsonConvertor, JsonConvertor>();

        var settings = new JsonSerializerSettings();
        settings.Converters = new List<JsonConverter>()
        {
            new JsonConvertor.AbstractConverter<CryptoBTC, ICryptoBTC>(),
            //new AbstractConverter<Thing2, IThingy2>()
        };
        //settings.Converters.Add(new CryptoCurrencyJsonConvertor());
        //settings.Converters.Add(new CryptoICurrencyJsonConvertor());
        services.AddSingleton(typeof(JsonSerializerSettings), settings);

        return services;
    }
}
