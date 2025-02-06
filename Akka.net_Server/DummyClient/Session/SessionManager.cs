using Google.Protobuf.Protocol;

using System;
using System.Collections.Generic;
using System.Text;

namespace DummyClient.Session
{
	public class SessionManager
	{
		public static SessionManager Instance { get; } = new SessionManager();

		HashSet<ServerSession> _sessions = new HashSet<ServerSession>();
		object _lock = new object();

		public ServerSession Generate()
		{
			lock (_lock)
			{
				ServerSession session = new ServerSession();

				_sessions.Add(session);
				Console.WriteLine($"Connected ({_sessions.Count}) Players");
				return session;
			}
		}

		public void Remove(ServerSession session)
		{
			lock (_lock)
			{
				_sessions.Remove(session);
				Console.WriteLine($"Connected ({_sessions.Count}) Players");
			}
		}

        public void FlushAllSessions()
        {
            foreach (var session in _sessions)
            {
                session.Send(new C_Chat() { Chat= $"{session.SessionId}번 User - TestMessage"});
            }
        }
    }
}
