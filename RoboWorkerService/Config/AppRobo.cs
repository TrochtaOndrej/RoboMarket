using Helper.Serialization;
using RoboWorkerService.Interfaces;

namespace RoboWorkerService.Config;

public class AppRobo : IAppRobo
{
    public IConfig Config { get; }
    public IConfiguration Configuration { get; }
    public IJsonFolderConfigAppRobo<RoboConfig> RoboConfig { get; }
    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    public AppRobo(IConfig config, IConfiguration configuration, IJsonFolderConfigAppRobo<RoboConfig> roboConfig)
    {
        Config = config;
        Configuration = configuration;
        RoboConfig = roboConfig;
    }

    public void CallCancelAppToken()
    {
        _cancellationTokenSource.Cancel();
    }

    public CancellationToken GetAppToken()
    {
        return _cancellationTokenSource.Token;
    }

    public T GetService<T>()
    {
        return HostApp.Host.Services.GetRequiredService<T>();
    }
}