using Helper.Serialization;
using RoboWorkerService.Config;

namespace RoboWorkerService.Interfaces;

public interface IAppRobo
{
    void CallCancelAppToken();
    CancellationToken GetAppToken();
    IConfig Config { get; }
    IConfiguration Configuration { get; }
    IJsonFolderConfigAppRobo<RoboConfig> RoboConfig { get; }
    T GetService<T>();
}