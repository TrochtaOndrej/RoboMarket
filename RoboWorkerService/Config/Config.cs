namespace RoboWorkerService.Config;

public interface IConfig
{
    string ConfigPath { get; }
    string RootPath { get; }

    string ReportPath { get; }
    bool IsDevelopment { get; }
    Type DefineMarketAsType { get; }
    char CryptoSeparator { get; }
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
    private readonly string CurrentPath = Environment.CurrentDirectory;

    public Config(IConfiguration configuration, ILogger<Config> logger)
    {
        var namespaceExchange = configuration.GetValue<string>("RoboApp:ActualMarket");
        DefineMarketAsType = GetTypeExchangeFromString(namespaceExchange);

        var appPath = configuration.GetValue<string>("RoboApp:RoboDataPath");
        CryptoSeparator = configuration.GetValue<char>("RoboApp:CryptoSeparator");
        if (!string.IsNullOrEmpty(appPath)) CurrentPath = appPath;

        Directory.CreateDirectory(ConfigPath);
        Directory.CreateDirectory(ReportPath);

        IsDevelopment = configuration.GetValue<bool>("RoboApp:IsDevelopment");
        logger.LogInformation("Root RoboData path: {Path}", RootPath);
    }

    public char CryptoSeparator { get; } = '-';

    public string RootPath => CurrentPath + @"RoboData\";
    public string ReportPath => RootPath + DefineMarketAsType.Name + @"\Reports\";
    public bool IsDevelopment { get; }
    public string ConfigPath => RootPath + DefineMarketAsType.Name + @"\Config\";

    public Type DefineMarketAsType { get; }

    private Type GetTypeExchangeFromString(string qualifyNamespace)
    {
        var type = Type.GetType(qualifyNamespace, false);
        if (type is null)
            throw new BussinesExceptions(
                $"Defined namespace in `appsettings.json`: {qualifyNamespace} is not possible load from Exchange!");
        return type;
    }
}