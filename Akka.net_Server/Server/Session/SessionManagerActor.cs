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
        public class SetRoomManagerActor 
        {
            public IActorRef RoomManager { get; }
            public SetRoomManagerActor(IActorRef roomManager) => RoomManager = roomManager;
        }
        public class GenerateSession 
        {
            public Socket SessionSocket { get; set; }
            public GenerateSession(Socket sessionSocekt)
            {
                SessionSocket = sessionSocekt;
            }
        }
        public class FindSession
        {
            public int SessionID { get; }
            public FindSession(int sessionID) => SessionID = sessionID;
        }
        public class RemoveSession
        {
            public ClientSession Session { get; }
            public RemoveSession(ClientSession session) => Session = session;
        }
        public class GetAllSessions { }
        #endregion

        IActorRef _roomManagerActor;
        int _sessionID = 0;
        Dictionary<int, ClientSession> _sessions = new Dictionary<int, ClientSession>();

        public SessionManagerActor()
        {
            Receive<SetRoomManagerActor>(msg => _roomManagerActor = msg.RoomManager);

            Receive<GenerateSession>(msg => GenerateSessionHandle(msg));
            Receive<FindSession>(msg => FindSessionHandle(msg));
            Receive<RemoveSession>(msg => RemoveSessionHandle(msg));
            Receive<GetAllSessions>(_ => GetAllSessionsHandle());
        }

        // 세션 생성
        private void GenerateSessionHandle(GenerateSession msg)
        {
            var clientSession = new ClientSession();
            clientSession.SessionID = ++_sessionID;
            _sessions.Add(clientSession.SessionID, clientSession);

            clientSession.Start(msg.SessionSocket);
            clientSession.OnConnected(msg.SessionSocket.RemoteEndPoint);
        }

        // 세션 조회
        private void FindSessionHandle(FindSession msg)
        {
            _sessions.TryGetValue(msg.SessionID, out var clientSession);
            Sender.Tell(clientSession); // 결과 반환
        }

        // 세션 제거
        private void RemoveSessionHandle(RemoveSession msg)
        {
            if (msg.Session != null)
            {
                _sessions.Remove(msg.Session.SessionID);
            }
        }

        // 모든 세션 조회
        private void GetAllSessionsHandle()
        {
            var allSessions = _sessions.Values.ToList();
            Sender.Tell(allSessions); // 세션 목록 반환
        }
    }
}
