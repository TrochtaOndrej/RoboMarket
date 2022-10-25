using RoboWorkerService.Interfaces;

namespace RoboWorkerService.Config;

public class AppRobo : IAppRobo
{
    public IConfig Config { get; }
    public IConfiguration Configuration { get; }
    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    public AppRobo(IConfig config, IConfiguration configuration)
    {
        Config = config;
        Configuration = configuration;
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