using Akka.Actor;
using Akka.Cluster;
using Akka.Configuration;
using Akka.Configuration.Hocon;
using Akka.LogServer;

using Serilog;

using System.Configuration;

namespace Akka.LogAkka.Server
{
    public class Program
    {
        static ActorSystem ServerActorSystem;
        static void Main(string[] args)
        {
            ConfigManager.LoadConfig();
            
            #region Serilog Logger 정의
            SerilogManager.Init();//Log.Logger 정의
            #endregion

            #region Cluster 활성화
            var config = ConfigurationFactory.ParseString(File.ReadAllText("hocon.conf"));
            ServerActorSystem = ActorSystem.Create("ClusterSystem", config);

            var LogManagerActor = ServerActorSystem.ActorOf(Props.Create(() => new LogManagerActor()), "LogManagerActor");
            #endregion

            Log.Logger.Information($"==========LogServer OPEN==========");
            Log.Logger.Information("로그 경로 : Debuf/logs/xx.json");
            ServerActorSystem.WhenTerminated.Wait();
            Log.CloseAndFlush();
        }
    }
}
