using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Text;

using Akka.Actor;
using Akka.Configuration;
using Akka.Configuration.Hocon;
using Akka.Event;
using Akka.Routing;

using Google.Protobuf.Protocol;

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
            #endregion

            #region Actor
            var clusterListenerActor =  ServerActorSystem.ActorOf(Props.Create(() => new ClusterListenerActor()), "clusterListenerActor");
            var roomManager = ServerActorSystem.ActorOf(Props.Create(() => new RoomManagerActor()), "RoomManagerActor");
            var sessionManager = ServerActorSystem.ActorOf(Props.Create(() => new SessionManagerActor()), "SessionManagerActor");

            sessionManager.Tell(new SessionManagerActor.SetRoomManagerActor(roomManager));
            roomManager.Tell(new RoomManagerActor.SetSessionManager(sessionManager));
            #endregion

            #region IOCP Server Start
            Console.WriteLine("==========Listener OPEN==========");
            Console.WriteLine("Listener....");
            ThreadPool.GetMaxThreads(out int workerThreads, out int completionPortThreads);
            Console.WriteLine($"Max Worker Threads: {workerThreads}, Max IOCP Threads: {completionPortThreads}");

            string hostName = Dns.GetHostName();
            IPHostEntry ipEntry = Dns.GetHostEntry(hostName);
            IPAddress ipAddr = ipEntry.AddressList[1];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 8888);

            _listener.Init(endPoint, (socket) =>
            {
                sessionManager.Tell(new SessionManagerActor.GenerateSession(socket));
            });
            #endregion
            
            ServerActorSystem.WhenTerminated.Wait();
        }
    }
}
