using CryptoBotCore.API;
using Helper.Interface;
using Helper.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RoboWorkerService.Config;
using RoboWorkerService.Csv;
using RoboWorkerService.Interface;
using RoboWorkerService.Interfaces;
using RoboWorkerService.Json;
using RoboWorkerService.Market;
using RoboWorkerService.Market.Enum;
using RoboWorkerService.Market.Model;
using RoboWorkerService.Market.Processing;
using RoboWorkerService.Market.Processing.DefineMoney;
using RoboWorkerService.Robo;


namespace RoboWorkerService;

public static class RoboServices
{
    public static IServiceCollection AddRoboServices(this IServiceCollection services)
    {
        services.AddSingleton<IConfig, Config.Config>();
        services.AddSingleton<IAppRobo, AppRobo>();
        services.AddSingleton<ICoinmateAPI, CoinmateAPI>();
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

        services.AddSingleton(typeof(ITransactionDataDriver<>), typeof(TransactionsDataDriver<>));
        services.AddSingleton(typeof(IMarketTransactionCsv<>), typeof(MarketTransactionCsv<>));

        return services;
    }


    public static IServiceCollection AddMarketBurker<T>(this IServiceCollection services) where T : ICryptoCurrency
    {
        services.AddSingleton<IMarketCoreCupBroker<T>, MarketCoreCupBroker<T>>();
        services.AddSingleton<ICupProcessingMarket<T>, CupProcessingMarket<T>>();

        services.AddSingleton<IDefinedMoneyProcessMarket<T>, SharpProcessingMarket<T>>();
        services.AddSingleton<IMarketCoreSharpBroker<T>, MarketCoreDefinedSharpBroker<T>>();
        services.AddSingleton<ICoinMateRobo<T>, CoinMateRobo<T>>();
        services.AddSingleton<IBrokerMoneyExtraDataFile<T>, BrokerMoneyExtraDataFile<T>>();
        services.AddSingleton<IBrokerMoneyProcessExtraDataService<T>, BrokerMoneyProcessExtraDataService<T>>();

        return services;
    }

    public static IServiceCollection AddJsonService(this IServiceCollection services)
    {
        services.AddSingleton<IJsonConvertor, JsonConvertor>();
        services.AddSingleton(typeof(IJsonFolderConfigAppRobo<>), typeof(JsonFolderConfigAppRobo<>));

        JsonSerializerSettings serializerSettings = new JsonSerializerSettings
        {
            FloatParseHandling = FloatParseHandling.Decimal,
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            },
            Converters = new List<JsonConverter>()
            {
                new AbstractConverter<CryptoBTC, ICryptoBTC>(),
                new AbstractConverter<Wallet, IWallet>(),
            }
        };

        services.AddSingleton(typeof(JsonSerializerSettings), serializerSettings);
        return services;
    }
}