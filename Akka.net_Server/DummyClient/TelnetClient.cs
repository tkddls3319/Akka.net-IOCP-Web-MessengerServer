using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Akka.Actor;
using Akka.IO;

namespace DummyClient
{
    class TelnetClient : UntypedActor
    {
        public TelnetClient(string host, int port)
        {
            var endpoint = new DnsEndPoint(host, port);
            Context.System.Tcp().Tell(new Tcp.Connect(endpoint));
        }

        protected override void OnReceive(object message)
        {
            if (message is Tcp.Connected connected)
            {
                Console.WriteLine("Connected to {0}", connected.RemoteAddress);
                Sender.Tell(new Tcp.Register(Self));
                ReadConsoleAsync();
                Become(Connected(Sender));
            }
            else if (message is Tcp.CommandFailed)
            {
                Console.WriteLine("Connection failed");
            }
            else Unhandled(message);
        }

        private UntypedReceive Connected(IActorRef connection)
        {
            return message =>
            {
                if (message is Tcp.Received received)  // data received from network
                {
                    Console.WriteLine(Encoding.ASCII.GetString(received.Data.ToArray()));
                }
                else if (message is string s)   // data received from console
                {
                    connection.Tell(Tcp.Write.Create(ByteString.FromString(s + "\n")));
                    ReadConsoleAsync();
                }
                else if (message is Tcp.PeerClosed)
                {
                    Console.WriteLine("Connection closed");
                }
                else Unhandled(message);
            };
        }

        private void ReadConsoleAsync()
        {
            Task.Factory.StartNew(self => Console.In.ReadLineAsync().PipeTo((ICanTell)self), Self);
        }
    }
}
