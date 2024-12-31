using System.Net;

using Akka.Actor;

namespace DummyClient
{
    internal class Program
    {
        public static ActorSystem ClientActors;

        static void Main(string[] args)
        {
            Console.WriteLine("Client Start");

            ClientActors = ActorSystem.Create("ClientActors");

            string hostName = Dns.GetHostName();
            IPHostEntry ipEntry = Dns.GetHostEntry(hostName);
            IPAddress ipAddr = ipEntry.AddressList[1];

            ClientActors.ActorOf(Props.Create(() => new TelnetClient(ipAddr.ToString(), 8888)), "ClientActors");

            ClientActors.WhenTerminated.Wait();
        }
    }
}
