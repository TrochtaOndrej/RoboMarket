﻿using RoboWorkerService.Config;

namespace RoboWorkerService.Interfaces;

public interface IAppRobo
{
    void CallCancelAppToken();
    CancellationToken GetAppToken();
    IConfig Config { get; }
    IConfiguration Configuration { get; }
    T GetService<T>();
}