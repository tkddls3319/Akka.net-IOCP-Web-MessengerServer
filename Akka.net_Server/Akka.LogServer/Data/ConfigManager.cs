using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Akka.LogServer
{
    [Serializable]
    public class LogConfig
    {
        public string logPath;
    }

    public class ConfigManager
    {
        public static LogConfig Config { get; set; }
        public static void LoadConfig()
        {
            string text = File.ReadAllText("config.json");
            Config = Newtonsoft.Json.JsonConvert.DeserializeObject<LogConfig>(text);    
        }
    }
}
