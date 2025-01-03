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
            ServerActorSystem = ActorSystem.Create("ServerActorSystem");

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

            _listener.Init(endPoint, (socket) =>
            {
                sessionManager.Tell(new SessionManagerActor.GenerateSession(socket));
            });


            ServerActorSystem.WhenTerminated.Wait();
        }
    }
}
