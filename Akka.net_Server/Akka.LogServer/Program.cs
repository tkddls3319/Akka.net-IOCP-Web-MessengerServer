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
            try
            {
                ConfigManager.LoadConfig();

                #region Serilog Logger 정의
                SeriLogManager.Init();//Log.Logger 정의
                #endregion

                #region Cluster 활성화
                var config = ConfigurationFactory.ParseString(File.ReadAllText("hocon.conf"));
                ServerActorSystem = ActorSystem.Create(Enum.GetName(ActtorType.ClusterSystem), config);

                var logManagerActor = ServerActorSystem.ActorOf(Props.Create(() => new LogManagerActor()), Enum.GetName(ActtorType.LogManagerActor));
                #endregion

                Log.Logger.Information($"==========LogServer OPEN==========");
                Log.Logger.Information("로그 경로 : Debuf/logs/xx.json");

                ServerActorSystem.WhenTerminated.Wait();
                Log.CloseAndFlush();

            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
