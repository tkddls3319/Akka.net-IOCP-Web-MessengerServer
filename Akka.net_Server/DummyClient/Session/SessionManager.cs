using Google.Protobuf.Protocol;
using Google.Protobuf.WellKnownTypes;

using ServerCore;

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
                int count = _sessions.Count / 3;
                if (count == 0 && _sessions.Count > 0) count = 1;

                List<ServerSession> selectedSessions = _sessions.Where(s=>s.IsRoomEnter).OrderBy(x => _random.Next()).Take(count).ToList();

                Parallel.ForEach(selectedSessions, new ParallelOptions { MaxDegreeOfParallelism = 100 }, session =>
                {
                    if (session.IsRoomEnter)
                    {
                        var time = Timestamp.FromDateTime(DateTime.UtcNow);
                        session.Send(new C_Chat() { Chat = $"{session.SessionId}번 User - TestMessage", Time = time });
                    }
                });
            }
        }
    }
}
