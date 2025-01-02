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
    //public class Program
    //{
    //    static Listener _listener = new Listener();
    //    //public static ActorSystem ServerActorSystem;
    //    static void Main(string[] args)
    //    {
    //        //ServerActorSystem = ActorSystem.Create("ServerActorSystem");

    //        //var hostName = Dns.GetHostName();
    //        //var ipEntry = Dns.GetHostEntry(hostName);
    //        //IPAddress ipAddr = ipEntry.AddressList[1];
    //        //var endPoint = new IPEndPoint(ipAddr, 8888);

    //        //var sessionManager = ServerActorSystem.ActorOf(Props.Create(() => new SessionManagerActor()), "SessionManager");
    //        //var listener = ServerActorSystem.ActorOf(Props.Create(() => new ListenerActor(sessionManager, endPoint)), "Listener");

    //        //Console.WriteLine("Server is running. Press ENTER to exit.");
    //        //Console.ReadLine();
    //        //ServerActorSystem.WhenTerminated.Wait();

    //        RoomManager.Instance.Push(() => { RoomManager.Instance.Add(); });

    //        string hostName = Dns.GetHostName();

    //        IPHostEntry ipEntry = Dns.GetHostEntry(hostName);
    //        IPAddress ipAddr = ipEntry.AddressList[1];
    //        IPEndPoint endPoint = new IPEndPoint(ipAddr, 8888);

    //        Console.WriteLine("==========Server OPEN==========");
    //        Console.WriteLine("Listener....");
    //        _listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });

    //        {
    //            Thread t1 = new Thread(NetWorkTask);
    //            t1.Name = "NetWorkTask Send";
    //            t1.Start();
    //        }

    //        Thread.CurrentThread.Name = "RoomManager";
    //        RoomManagerTask();
    //    }
    //}
    public class Program
    {
        static Listener _listener = new Listener();
        public static ActorSystem ServerActorSystem;
        static void Main(string[] args)
        {
            ServerActorSystem = ActorSystem.Create("ServerActorSystem");

            var roomManager = ServerActorSystem.ActorOf(Props.Create(() => new RoomManagerActor()), "RoomManagerActor");
            //roomManager.Tell(new RoomManagerActor.AddRoom());

            string hostName = Dns.GetHostName();

            IPHostEntry ipEntry = Dns.GetHostEntry(hostName);
            IPAddress ipAddr = ipEntry.AddressList[1];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 8888);

            Console.WriteLine("==========Server OPEN==========");
            Console.WriteLine("Listener....");
            _listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });


            ServerActorSystem.WhenTerminated.Wait();
        }
    }
}
