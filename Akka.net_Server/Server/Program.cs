using System.Net;

using Akka.Actor;

namespace Server
{
    public class Program
    {
        public static ActorSystem ServerActors;
        static void Main(string[] args)
        {
            Console.WriteLine("Server Start");
            
            ServerActors = ActorSystem.Create("ServerActors");

            var sessionManagerActor = ServerActors.ActorOf(Props.Create(() => new SessionManager()), "SessionManagerActor");

            ServerActors.ActorOf(Props.Create(() => new Listener(new IPEndPoint(IPAddress.Any, 9999), sessionManagerActor)), "ServerActors");

            ServerActors.WhenTerminated.Wait();
        }
    }
}
