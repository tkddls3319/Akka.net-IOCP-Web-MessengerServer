using Akka.Actor;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;


namespace Server
{
    public class SessionManagerActor : ReceiveActor
    {
        #region message
        public class Generate
        {
            public Socket AcceptSocket;
            public Generate(Socket acceptSocket)
            {
                AcceptSocket = acceptSocket;
            }
        }
        public class Remove
        {
            public int SessionId { get; }

            public Remove(int sessionId)
            {
                SessionId = sessionId;
            }
        }
        #endregion

        private int _sessionIdCounter;
        private readonly Dictionary<int, IActorRef> _sessions = new Dictionary<int, IActorRef>();
        public SessionManagerActor()
        {
            Receive<Generate>(message => GenerateHandler(message));
            Receive<Remove>(message => RemoveHandler(message));
        }
        private void GenerateHandler(Generate message)
        {
            var sessionId = ++_sessionIdCounter;

            var sessionActor = Context.ActorOf(Props.Create(() => new SessionActor(message.AcceptSocket, sessionId)));
            _sessions[sessionId] = sessionActor;


            Console.WriteLine($"Session {sessionId} Generate.");
        }
        private void RemoveHandler(Remove message)
        {
            if (_sessions.TryGetValue(message.SessionId, out var sessionActor))
            {
                Context.Stop(sessionActor);
                _sessions.Remove(message.SessionId);
                Console.WriteLine($"Session {message.SessionId} ended.");
            }
            else
            {
                Console.WriteLine($"No session found with ID: {message.SessionId}");
            }
        }
    }
}
