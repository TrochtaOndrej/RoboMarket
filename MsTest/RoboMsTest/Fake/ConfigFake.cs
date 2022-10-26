using ExchangeSharp;
using RoboWorkerService.Config;

namespace RoboMsTest;

public class ConfigFake : IConfig
{
    public string ConfigPath => RootPath + @"\Test\Config\";
    public string RootPath => Environment.CurrentDirectory;
    public string ReportPath => ConfigPath + @"Report\";
    public bool IsDevelopment { get; } = true;
    public Type DefineMarketAsType => typeof(ExchangeCoinbaseAPI);

    public ConfigFake()
    {
        Directory.CreateDirectory(ConfigPath);
        Directory.CreateDirectory(ReportPath);
    }
}