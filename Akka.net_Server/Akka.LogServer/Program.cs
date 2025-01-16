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
            #region Logger 정의
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File($"logs/chatLog.json", rollingInterval: RollingInterval.Day)
                .CreateLogger();
            #endregion

            #region Cluster 활성화
            var config = ConfigurationFactory.ParseString(File.ReadAllText("hocon.conf"));
            ServerActorSystem = ActorSystem.Create("ClusterSystem", config);

            var LogManagerActor = ServerActorSystem.ActorOf(Props.Create(() => new LogManagerActor()), "LogManagerActor");
            #endregion

            ServerActorSystem.WhenTerminated.Wait();
            Log.CloseAndFlush();
        }
    }
}
