﻿using ExchangeSharp;
using Helper.Serialization;

namespace RoboWorkerService.Config
{
    public interface IConfig
    {
        string ConfigPath { get; }
        string RootPath { get; }

        string ReportPath { get; }
        bool IsDevelopment { get; }
        Type DefineMarketAsType { get; }

        int WaitingBetweenStrategyInMiliSeconds { get; }

        string TelegramToken { get; }
        string ComplileEnvironment { get; }
    }

    public record RoboConfig
    {
        public const string NumberOrderName = nameof(NumberOrder);
        public int NumberOrder { get; set; }

        public int GetNumberOrder()
        {
            return NumberOrder++;
        }
    }

    public class Config : IConfig
    {
        private readonly string CurrentPath = System.Environment.CurrentDirectory;

        public string RootPath => $@"{CurrentPath}RoboData\";
        public string ReportPath => $@"{RootPath}{DefineMarketAsType.Name}\Reports\";
        public bool IsDevelopment { get; }
        public string ConfigPath => $@"{RootPath}{DefineMarketAsType.Name}\Config\";

        public Type DefineMarketAsType { get; }
        public int WaitingBetweenStrategyInMiliSeconds { get; } = 5;
        public string TelegramToken { get; }
        public string ComplileEnvironment { get; }

        public Config(IConfiguration configuration, ILogger<Config> logger)
        {
#if (DEBUG)
            ComplileEnvironment = "Test";
#else
            ComplileEnvironment = "Prod";
#endif

            var namespaceExchange = configuration.GetValue<string>("RoboApp:ActualMarket");
            DefineMarketAsType = GetTypeExchangeFromString(namespaceExchange);

            var appPath = configuration.GetValue<string>("RoboApp:RoboDataPath");
            WaitingBetweenStrategyInMiliSeconds = configuration.GetValue<int>("RoboApp:WaitingBetweenStrategyInSec") * 1000;
            TelegramToken = configuration.GetValue<string>("RoboApp:TelegramToken");
            if (!string.IsNullOrEmpty(appPath)) CurrentPath = appPath;

            Directory.CreateDirectory(ConfigPath);
            Directory.CreateDirectory(ReportPath);

            IsDevelopment = configuration.GetValue<bool>("RoboApp:IsDevelopment");
            logger.LogInformation("Root RoboData path: {Path}", RootPath);
        }

        private Type GetTypeExchangeFromString(string qualifyNamespace)
        {
            try
            {
                var type = Type.GetType(qualifyNamespace, false);
                if (type is null)
                    throw new BussinesExceptions(
                        $"Defined namespace in `appsettings.json`: {qualifyNamespace} is not possible load from Exchange!");
                return type;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"You can try this:{typeof(ExchangeCoinbaseAPI).AssemblyQualifiedName}");
                throw;
            }
        }
    }
}