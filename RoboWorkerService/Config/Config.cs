namespace RoboWorkerService.Config
{
    public interface IConfig
    {
        string ConfigPath { get; }
        string RootPath { get; }
    }

    public class Config : IConfig
    {
        private readonly string CurrentPath = Environment.CurrentDirectory;

        public string RootPath => CurrentPath + @"RoboData\";
        public string ConfigPath => RootPath + @"Config\";
       
        public Config(IConfiguration configuration, ILogger<Config> logger)
        {
            var appPath = configuration.GetValue<string>("RoboApp:RoboDataPath");
            if (!string.IsNullOrEmpty(appPath)) CurrentPath = appPath;
            Directory.CreateDirectory(ConfigPath);
            logger.LogInformation("Root RoboData path: {Path}", RootPath);
        }
    }
}