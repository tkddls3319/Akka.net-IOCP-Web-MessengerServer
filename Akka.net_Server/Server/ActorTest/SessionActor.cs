using Akka.Actor;
using Akka.Remote;

using Server.ServerCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class SessionActor : ReceiveActor
    {
        private readonly IActorRef _sendActor;
        private readonly IActorRef _receiveActor;
        private readonly Socket _socket;
        public int SessionId { get; }

        public SessionActor(Socket socket, int sessionId)
        {
            _socket = socket;
            SessionId = sessionId;

            _sendActor = Context.ActorOf(Props.Create(() => new SendActor(_socket)), "SendActor");
            _receiveActor = Context.ActorOf(Props.Create(() => new RecvActor(_socket)), "ReceiveActor");

            Receive<ArraySegment<byte>>(msg => HandleSend(msg));
        }

        private void HandleSend(ArraySegment<byte> data)
        {
            _sendActor.Tell(data);
        }

        protected override void PreStart()
        {
            Console.WriteLine($"Session {SessionId} started.");
        }

        protected override void PostStop()
        {
            Console.WriteLine($"Session {SessionId} stopped.");
            _socket.Close();
        }
    }
}
