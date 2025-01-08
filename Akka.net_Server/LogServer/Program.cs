using Akka.Actor;
using Akka.Cluster;
using Akka.Configuration;
using Akka.Configuration.Hocon;

using System.Configuration;

namespace LogServer
{
    internal class Program
    {
        static ActorSystem ServerActorSystem;
        static void Main(string[] args)
        {
            var config = ConfigurationFactory.ParseString(File.ReadAllText("hocon.conf"));
            ServerActorSystem = ActorSystem.Create("ClusterSystem", config);

            var backend = ServerActorSystem.ActorOf(Props.Create(() => new BackendActor(ServerActorSystem)), "Backend");

            ServerActorSystem.WhenTerminated.Wait();
        }
    }
}
