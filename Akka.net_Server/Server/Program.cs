using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Text;

using Akka.Actor;
using Akka.Configuration;
using Akka.Configuration.Hocon;
using Akka.Event;

using ServerCore;

namespace Server
{
    public class Program
    {
        static Listener _listener = new Listener();
        public static ActorSystem ServerActorSystem;
        static void Main(string[] args)
        {
            //ServerActorSystem = ActorSystem.Create("ServerActorSystem");

            #region cluster
            // XML에서 HOCON 설정 읽기
            var section = (AkkaConfigurationSection)ConfigurationManager.GetSection("akka");
            var config = section.AkkaConfig;
            ServerActorSystem = ActorSystem.Create("ClusterSystem", config);

            var backendAddress = Address.Parse("akka.tcp://ClusterSystem@localhost:5001");
            var backend = ServerActorSystem.ActorSelection($"{backendAddress}/user/BackendActor").Anchor;

            var frontend = ServerActorSystem.ActorOf(Props.Create(() => new FrontendActor(backend)), "FrontendActor");

            // 클라이언트 요청 시뮬레이션
            for (int i = 1; i <= 5; i++)
            {
                frontend.Tell($"Request {i}");
                Thread.Sleep(500); // 지연 시간
            }

            #endregion

            var roomManager = ServerActorSystem.ActorOf(Props.Create(() => new RoomManagerActor()), "RoomManagerActor");
            var sessionManager = ServerActorSystem.ActorOf(Props.Create(() => new SessionManagerActor()), "SessionManagerActor");

            sessionManager.Tell(new SessionManagerActor.SetRoomManagerActor(roomManager));
            roomManager.Tell(new RoomManagerActor.SetSessionManager(sessionManager));

            string hostName = Dns.GetHostName();
            IPHostEntry ipEntry = Dns.GetHostEntry(hostName);
            IPAddress ipAddr = ipEntry.AddressList[1];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 8888);

            Console.WriteLine("==========Server OPEN==========");
            Console.WriteLine("Listener....");

            ThreadPool.GetMaxThreads(out int workerThreads, out int completionPortThreads);
            Console.WriteLine($"Max Worker Threads: {workerThreads}, Max IOCP Threads: {completionPortThreads}");

            _listener.Init(endPoint, (socket) =>
            {
                sessionManager.Tell(new SessionManagerActor.GenerateSession(socket));
            });


            ServerActorSystem.WhenTerminated.Wait();
        }
    }
}
