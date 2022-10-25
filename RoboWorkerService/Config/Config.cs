namespace RoboWorkerService.Config
{
    public interface IConfig
    {
        string ConfigPath { get; }
        string RootPath { get; }

        string ReportPath { get; }
        bool IsDevelopment { get; }
        Type DefineMarketAsType { get; }
    }

    public class Config : IConfig
    {
        private readonly string CurrentPath = Environment.CurrentDirectory;

        public string RootPath => CurrentPath + @"RoboData\";
        public string ReportPath => RootPath + DefineMarketAsType.Name + @"\Reports\";
        public bool IsDevelopment { get; }
        public string ConfigPath => RootPath + DefineMarketAsType.Name + @"\Config\";

        public Type DefineMarketAsType { get; }

        public Config(IConfiguration configuration, ILogger<Config> logger)
        {
            var namespaceExchange = configuration.GetValue<string>("RoboApp:ActualMarket");
            DefineMarketAsType = GetTypeExchangeFromString(namespaceExchange);

            var appPath = configuration.GetValue<string>("RoboApp:RoboDataPath");
            if (!string.IsNullOrEmpty(appPath)) CurrentPath = appPath;

            Directory.CreateDirectory(ConfigPath);
            Directory.CreateDirectory(ReportPath);

            IsDevelopment = configuration.GetValue<bool>("RoboApp:IsDevelopment");
            logger.LogInformation("Root RoboData path: {Path}", RootPath);
        }

        private Type GetTypeExchangeFromString(string qualifyNamespace)
        {
            var type = Type.GetType(qualifyNamespace, false);
            if (type is null)
                throw new BussinesExceptions(
                    $"Defined namespace in `appsettings.json`: {qualifyNamespace} is not possible load from Exchange!");
            return type;
        }
    }
}