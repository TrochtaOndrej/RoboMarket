using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboWorkerService.Config
{
    public interface IConfig
    {
        string ConfigPath { get; }
    }

    public class Config : IConfig
    {
        private readonly string CurrentPath = Environment.CurrentDirectory;

        public string ConfigPath => CurrentPath + @"\Setup\";

        public Config()
        {
            Directory.CreateDirectory(ConfigPath);
        }
    }
}
