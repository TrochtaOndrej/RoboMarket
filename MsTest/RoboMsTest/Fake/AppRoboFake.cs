using Helper.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RoboWorkerService.Config;
using RoboWorkerService.Interfaces;

namespace RoboMsTest;

public class AppRoboFake : IAppRobo
{
    private CancellationTokenSource ct = new CancellationTokenSource();

    public void CallCancelAppToken()
    {
        ct.Cancel();
    }

    public CancellationToken GetAppToken()
    {
        return ct.Token;
    }

    public IConfig Config { get; }
    public IConfiguration Configuration { get; }
    public IJsonFolderConfigAppRobo<RoboConfig> RoboConfig { get; }

    public T GetService<T>()
    {
        return BaseTest.ServiceProvider.GetRequiredService<T>();
    }

    public AppRoboFake(IConfig config, IConfiguration configuration, IJsonFolderConfigAppRobo<RoboConfig> roboConfig)
    {
        Config = config;
        Configuration = configuration;
        RoboConfig = roboConfig;
    }
}