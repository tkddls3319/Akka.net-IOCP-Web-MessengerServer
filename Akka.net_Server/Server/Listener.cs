using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;

using Akka.IO;

namespace Server
{
    public class Listener : UntypedActor
    {
        private readonly IActorRef _sessionManager;
        public Listener(IPEndPoint endPoint, IActorRef sessionManager)
        {
            _sessionManager = sessionManager;
            Context.System.Tcp().Tell(new Tcp.Bind(Self, endPoint, backlog: 1000));
        }

        protected override void OnReceive(object message)
        {
            if (message is Tcp.Bound bound)
            {
                Console.WriteLine(bound);
            }
           else if (message is Tcp.Connected connected)
            {
                Console.WriteLine("Client connected from " + connected.RemoteAddress);
                // SessionManager에 새 세션 시작 요청
                _sessionManager.Tell(new SessionManager.StartSession(Sender));

            }
            else if (message is Tcp.Received received)
            {
                var data = received.Data;
                Console.WriteLine("Received data: " + data.ToString());
            }
            else if (message is Tcp.CommandFailed)
            {
                Console.WriteLine("Command failed.");
            }
            else if (message is Tcp.PeerClosed)
            {
                Console.WriteLine("Peer closed connection.");
                Context.Stop(Self); // Actor 종료
            }
        }
    }
}
