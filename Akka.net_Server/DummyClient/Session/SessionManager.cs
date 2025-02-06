using Google.Protobuf.Protocol;
using Google.Protobuf.WellKnownTypes;

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
        private Random _random = new Random();
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
			lock (_lock)
			{
                int count = _sessions.Count / 3; // 전체 세션의 1/3만큼 선택
                if (count == 0 && _sessions.Count > 0) count = 1; // 최소 1개는 전송

                List<ServerSession> selectedSessions = _sessions.OrderBy(x => _random.Next()).Take(count).ToList();

                foreach (var session in selectedSessions)
                {
                    var time = Timestamp.FromDateTime(DateTime.UtcNow);

                    session.Send(new C_Chat() { Chat = $"{session.SessionId}번 User - TestMessage", Time = time });
                }
            }
        }
    }
}
