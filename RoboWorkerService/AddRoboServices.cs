﻿using Helper.Serialization;
using Newtonsoft.Json;
using RoboWorkerService.Config;
using RoboWorkerService.Interface;
using RoboWorkerService.Interfaces;
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
        //services.AddSingleton(typeof(IWallet<,>), typeof(GlobalWallet<,>));
 
        services.AddSingleton(typeof(ITransactionProcessing<>), typeof(TransactionProcessing<>));
        return services;
    }

    public static IServiceCollection AddMarketBurker<T>(this IServiceCollection services) where T : ICryptoCurrency
    {
        services.AddSingleton<IMarketCoreCupBroker<T>, MarketCoreCupBroker<T>>();
        services.AddSingleton<ICupProcessingMarket<T>, CupProcessingMarket<T>>();

        services.AddSingleton<IDefinedMoneyProcessMarket<T>, DefinedMoneyProcessMarket<T>>();
        services.AddSingleton<IMarketCoreDefinedMoneyBroker<T>, MarketCoreDefinedMoneyBroker<T>>();
        services.AddSingleton<ICoinMateRobo<T>, CoinMateRobo<T>>();

        return services;
    }

    public static IServiceCollection AddJsonService(this IServiceCollection services)
    {
        services.AddSingleton<IJsonConvertor, JsonConvertor>();

        var settings = new JsonSerializerSettings();
        settings.Converters = new List<JsonConverter>()
        {
            new AbstractConverter<CryptoBTC, ICryptoBTC>(),
            new AbstractConverter<Wallet, IWallet>(),
        };

        services.AddSingleton(typeof(JsonSerializerSettings), settings);
        return services;
    }
}
