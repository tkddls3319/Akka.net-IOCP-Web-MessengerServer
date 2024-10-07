using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Akka.Actor;
using Akka.IO;

namespace Server
{
    internal class Session : UntypedActor
    {
        private readonly IActorRef _connection;

        public Session(IActorRef connection)
        {
            _connection = connection;
            var welcomeMessage = ByteString.FromString("Welcome to the Echo Server!!!\n");
            _connection.Tell(Tcp.Write.Create(welcomeMessage));
        }

        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case Tcp.Received received:
                    var data = received.Data.ToString();
                    Console.WriteLine($"Session received data: {data}");
                    _connection.Tell(Tcp.Write.Create(received.Data));
                    break;

                case Tcp.PeerClosed :
                    Console.WriteLine("Client disconnected.");
                    Context.Parent.Tell(new SessionManager.EndSession(_connection));
                    Context.Stop(Self);
                    break;

                case Tcp.ErrorClosed:
                    Console.WriteLine("Client disconnected with error.");
                    // 세션 종료 처리
                    Context.Parent.Tell(new SessionManager.EndSession(_connection));
                    Context.Stop(Self);
                    break;
                default:
                    Unhandled(message);
                    break;
            }
        }
    }
}