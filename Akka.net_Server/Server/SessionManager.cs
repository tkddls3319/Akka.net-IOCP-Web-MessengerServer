using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Akka.Actor;
using Akka.IO;

namespace Server
{
   public class SessionManager : UntypedActor
    {
        #region Message
        public class StartSession
        {
            public IActorRef Connection { get; }

            public StartSession(IActorRef connection)
            {
                Connection = connection;
            }
        }

        public class EndSession
        {
            public IActorRef Connection { get; }

            public EndSession(IActorRef connection)
            {
                Connection = connection;
            }
        }
        #endregion

        private readonly Dictionary<IActorRef, IActorRef> _sessions = new Dictionary<IActorRef, IActorRef>();

        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case StartSession startSession:
                    Generate(startSession.Connection);
                    break;

                case EndSession endSession:
                    Remove(endSession.Connection);
                    break;

                default:
                    Unhandled(message);
                    break;
            }
        }
        private void Generate(IActorRef connection)
        {
            if (_sessions.ContainsKey(connection))
            {
                Console.WriteLine("Session already exists for this connection.");
                return;
            }

            var sessionActor = Context.ActorOf(Props.Create(() => new Session(connection)), $"session-{Guid.NewGuid()}");
            _sessions[connection] = sessionActor;

            // 세션 Actor를 Tcp 연결의 이벤트 핸들러로 등록
            connection.Tell(new Tcp.Register(sessionActor));

            Console.WriteLine("New session started.");
        }
        private void Remove(IActorRef connection)
        {
            if (_sessions.TryGetValue(connection, out var sessionActor))
            {
                Context.Stop(sessionActor);
                _sessions.Remove(connection);
                Console.WriteLine("Session ended.");
            }
        }
    }
}