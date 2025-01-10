using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Text;

using Akka.Actor;
using Akka.Configuration;
using Akka.Configuration.Hocon;
using Akka.Event;
using Akka.Routing;

using ServerCore;

namespace Akka.Server
{
    public class Program
    {
        static Listener _listener = new Listener();
        public static ActorSystem ServerActorSystem;
        static void Main(string[] args)
        {

            #region cluster
            var config = ConfigurationFactory.ParseString(File.ReadAllText("hocon.conf"));
            ServerActorSystem = ActorSystem.Create("ClusterSystem", config);

            var frontend = ServerActorSystem.ActorOf(Props.Create(() => new FrontendActor()), "Frontend");

            var backendAddress = Address.Parse("akka.tcp://ClusterSystem@localhost:5001");
            var backend = ServerActorSystem.ActorSelection($"{backendAddress}/user/Backend").Anchor;

            var cluster = Cluster.Cluster.Get(ServerActorSystem);
            Console.WriteLine($"Cluster Members: {string.Join(", ", cluster.State.Members)}");


            //var config = File.ReadAllText("app.conf");
            //var conf = ConfigurationFactory.ParseString(config).BoostrapApplication(new AppBootstrapConfig(false, true));

            //var actorSystem = ActorSystem.Create("AkkaTrader", conf);

            //var tradeRouter = actorSystem.ActorOf(
            //    Props.Empty.WithRouter(new ClusterRouterGroup(
            //        new ConsistentHashingGroup(new[] { "/user/orderbooks" },
            //            TradeEventConsistentHashMapping.TradeEventMapping),
            //        new ClusterRouterGroupSettings(10000, new[] { "/user/orderbooks" }, true, useRole: "trade-processor"))), "tradesRouter");
            // 클라이언트 요청 시뮬레이션
            //for (int i = 1; i <= 5; i++)
            //{
            //    frontend.Tell($"Request {i}");
            //    Thread.Sleep(500); // 지연 시간
            //}
            #endregion

            //var roomManager = Akka.ServerActorSystem.ActorOf(Props.Create(() => new RoomManagerActor()), "RoomManagerActor");
            //var sessionManager = Akka.ServerActorSystem.ActorOf(Props.Create(() => new SessionManagerActor()), "SessionManagerActor");

            //sessionManager.Tell(new SessionManagerActor.SetRoomManagerActor(roomManager));
            //roomManager.Tell(new RoomManagerActor.SetSessionManager(sessionManager));

            //string hostName = Dns.GetHostName();
            //IPHostEntry ipEntry = Dns.GetHostEntry(hostName);
            //IPAddress ipAddr = ipEntry.AddressList[1];
            //IPEndPoint endPoint = new IPEndPoint(ipAddr, 8888);

            //Console.WriteLine("==========Akka.Server OPEN==========");
            //Console.WriteLine("Listener....");

            //ThreadPool.GetMaxThreads(out int workerThreads, out int completionPortThreads);
            //Console.WriteLine($"Max Worker Threads: {workerThreads}, Max IOCP Threads: {completionPortThreads}");

            //_listener.Init(endPoint, (socket) =>
            //{
            //    sessionManager.Tell(new SessionManagerActor.GenerateSession(socket));
            //});


            ServerActorSystem.WhenTerminated.Wait();
        }
    }
}
