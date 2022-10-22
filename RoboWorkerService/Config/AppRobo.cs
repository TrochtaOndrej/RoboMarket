using RoboWorkerService.Interfaces;

namespace RoboWorkerService.Config;

public class AppRobo : IAppRobo
{
    public  IConfig Config { get; }
    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    public AppRobo(IConfig config)
    {
        Config = config;
    }
        
    public void CallCancelAppToken()
    {
        _cancellationTokenSource.Cancel();
    }

    public CancellationToken GetAppToken()
    {
        return _cancellationTokenSource.Token;
    }
}