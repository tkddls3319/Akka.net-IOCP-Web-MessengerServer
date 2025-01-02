using Akka.Actor;
using Akka.Routing;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    // Listener Actor
    public class ListenerActor : ReceiveActor
    {
        private readonly IActorRef _sessionManager;
        private Socket _listenerSocket;

        public ListenerActor(IActorRef sessionManager, IPEndPoint endPint)
        {
            _sessionManager = sessionManager;

            _listenerSocket = new Socket(endPint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _listenerSocket.Bind(endPint);
            _listenerSocket.Listen(1000);

            SocketAsyncEventArgs acceptArgs = new SocketAsyncEventArgs();
            acceptArgs.Completed += AcceptCompleted;

            RegisterAccept(acceptArgs);
        }
        void RegisterAccept(SocketAsyncEventArgs args)
        {
            args.AcceptSocket = null;
            try
            {
                bool pending = _listenerSocket.AcceptAsync(args);
                if (pending == false)
                    AcceptCompleted(null, args);
            }
            catch (Exception err)
            {
                Console.WriteLine(err);
            }
        }
        void AcceptCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
                _sessionManager.Tell(new SessionManagerActor.Generate(args.AcceptSocket));
            }
            else
            {
                Console.WriteLine("AcceptCopleted err");
            }

            RegisterAccept(args);
        }
    }
}
