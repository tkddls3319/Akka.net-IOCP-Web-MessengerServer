using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Text;

using Akka.Actor;
using Akka.ClusterCore;
using Akka.Configuration;
using Akka.Configuration.Hocon;
using Akka.Event;
using Akka.Routing;

using Google.Protobuf.Protocol;

using Serilog;

using ServerCore;

using static Akka.Server.Define;

namespace Akka.Server
{

    public class Program
    {
        static Listener _listener = new Listener();
        public static ActorSystem ServerActorSystem;
        static void Main(string[] args)
        {
            #region Logger 정의
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();
            #endregion

            #region cluster
            var config = ConfigurationFactory.ParseString(File.ReadAllText("hocon.conf"));
            ServerActorSystem = ActorSystem.Create(Enum.GetName(ActtorType.ClusterSystem), config);
            #endregion

            #region Actor
            var clusterListenerActor =  ServerActorSystem.ActorOf(Props.Create(() => new ClusterListenerActor()), Enum.GetName(ActtorType.clusterListenerActor));
            var sessionManager = ServerActorSystem.ActorOf(Props.Create(() => new SessionManagerActor()), Enum.GetName(ActtorType.SessionManagerActor));
            var roomManager = ServerActorSystem.ActorOf(Props.Create(() => new RoomManagerActor(sessionManager)), Enum.GetName(ActtorType.RoomManagerActor));

            sessionManager.Tell(new SessionManagerActor.SetRoomManagerActorCommand(roomManager));

            var deadLetterMonitor = ServerActorSystem.ActorOf(Props.Create(() => new DeadLetterMonitor()), "deadLetterMonitor");
            // DeadLetter 감지를 위해 이벤트 스트림 구독
            ServerActorSystem.EventStream.Subscribe(deadLetterMonitor, typeof(DeadLetter));
            #endregion

            #region IOCP Server Start
            Log.Logger.Information("==========Listener OPEN==========");
            ThreadPool.GetMaxThreads(out int workerThreads, out int completionPortThreads);
            Log.Logger.Information($"Max Worker Threads: {workerThreads}, Max IOCP Threads: {completionPortThreads}");
            Log.Logger.Information("Listener....");

            string hostName = Dns.GetHostName();
            IPHostEntry ipEntry = Dns.GetHostEntry(hostName);
            IPAddress ipAddr = ipEntry.AddressList[1];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 8888);

            _listener.Init(endPoint, (socket) =>
            {
                sessionManager.Tell(new SessionManagerActor.GenerateSessionCommand(socket));
            });
            #endregion
            
            ServerActorSystem.WhenTerminated.Wait();
        }
    }
}
