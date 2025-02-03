using Serilog;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Akka.LogServer
{
    public static class SeriLogManager
    {
        static Dictionary<string, ILogger> _loggers = new();

        static object _lock = new object();

        static SeriLogManager()
        {
            Init();
        }
        public static void Init()
        {
            Log.Logger = new LoggerConfiguration()
          .MinimumLevel.Debug()
          .WriteTo.Console()
          .CreateLogger();
            //Log.Debug("This is a debug message.");
            //Log.Information("Application started at {Time}", DateTime.Now);
            //Log.Warning("Warning! Something might go wrong.");
            //Log.Error("An error occurred: {ErrorCode}", 404);
        }
        static ILogger Create(string name)
        {
            lock (_lock)
            {
                if (_loggers.TryAdd(name, new LoggerConfiguration()
                 .MinimumLevel.Debug()
                 .WriteTo.Console()
                 .WriteTo.File($"logs/{name}_.json", rollingInterval: RollingInterval.Day)
                 .CreateLogger()) == false)
                {
                    Log.Logger.Information($"[SerilogManager] CreateLog Fail : {name}");
                }
            }

            return _loggers[name];
        }
        public static ILogger Get(string name)
        {
            if (_loggers.TryGetValue(name, out var logger))
                return logger;
            else
                return Create(name);
        }
    }
}
