using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Text;

using Akka.Actor;
using Akka.Configuration;
using Akka.Configuration.Hocon;
using Akka.Event;

namespace Server
{
    public class Program
    {
        public static ActorSystem ServerActorSystem;
        static void Main(string[] args)
        {
            ServerActorSystem = ActorSystem.Create("ServerActorSystem");

            var hostName = Dns.GetHostName();
            var ipEntry = Dns.GetHostEntry(hostName);
            IPAddress ipAddr = ipEntry.AddressList[1];
            var endPoint = new IPEndPoint(ipAddr, 8888);

            var sessionManager = ServerActorSystem.ActorOf(Props.Create(() => new SessionManagerActor()), "SessionManager");
            var listener = ServerActorSystem.ActorOf(Props.Create(() => new ListenerActor(sessionManager, endPoint)), "Listener");

            Console.WriteLine("Server is running. Press ENTER to exit.");
            Console.ReadLine();
            ServerActorSystem.WhenTerminated.Wait();
        }
    }
}
