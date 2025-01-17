using Akka.Actor;

using Serilog;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Akka.Server
{
    public class SessionManagerActor : ReceiveActor, IWithTimers
    {
        #region message
        public class MsgSetRoomManagerActor
        {
            public IActorRef RoomManager { get; }
            public MsgSetRoomManagerActor(IActorRef roomManager) => RoomManager = roomManager;
        }
        public class MsgGenerateSession
        {
            public Socket SessionSocket { get; set; }
            public MsgGenerateSession(Socket sessionSocekt)
            {
                SessionSocket = sessionSocekt;
            }
        }
        public class MsgFindSession
        {
            public int SessionID { get; }
            public MsgFindSession(int sessionID) => SessionID = sessionID;
        }
        public class MsgRemoveSession
        {
            public ClientSession Session { get; }
            public MsgRemoveSession(ClientSession session) => Session = session;
        }
        public class MsgGetAllSessions { }
        public class MsgFlushSendAll { }

        // 타이머 키와 메시지
        private sealed class MsgTimerKey
        {
            public static readonly MsgTimerKey Instance = new();
            private MsgTimerKey() { }
        }
        private sealed class MsgTimerMessage
        {
            public static readonly MsgTimerMessage Instance = new();
            private MsgTimerMessage() { }
        }
        // 타이머 설정
        #endregion

        #region Actor
        IActorRef _roomManager;
        public ITimerScheduler Timers { get; set; } = null!;
        #endregion

        int _sessionID = 0;
        Dictionary<int, ClientSession> _sessions = new Dictionary<int, ClientSession>();

        public SessionManagerActor()
        {
            Receive<MsgSetRoomManagerActor>(msg => _roomManager = msg.RoomManager);

            Receive<MsgGenerateSession>(msg => GenerateSessionHandle(msg));
            Receive<MsgFindSession>(msg => FindSessionHandle(msg));
            Receive<MsgRemoveSession>(msg => RemoveSessionHandle(msg));
            Receive<MsgGetAllSessions>(_ => GetAllSessionsHandle());

            // 초기화: 타이머 시작
            Timers.StartPeriodicTimer(
                key: MsgTimerKey.Instance,
                msg: MsgTimerMessage.Instance,
                initialDelay: TimeSpan.FromMilliseconds(100),  // 초기 지연 시간
                interval: TimeSpan.FromMilliseconds(100));    // 주기적 실행 간격

            // 타이머 메시지 처리
            Receive<MsgTimerMessage>(_ => FlushAllSessions());
        }

        // 세션 생성
        private void GenerateSessionHandle(MsgGenerateSession msg)
        {
            var clientSession = new ClientSession(_roomManager);
            clientSession.SessionID = ++_sessionID;
            _sessions.Add(clientSession.SessionID, clientSession);

            clientSession.Start(msg.SessionSocket);
            clientSession.OnConnected(msg.SessionSocket.RemoteEndPoint);

            Log.Logger.Information($"[SessionManager] Generate Session Comp Session Count : {GetAllSessions().Count()}");
        }

        // 세션 조회
        private void FindSessionHandle(MsgFindSession msg)
        {
            _sessions.TryGetValue(msg.SessionID, out var clientSession);
            Sender.Tell(clientSession); // 결과 반환
        }

        // 세션 제거
        private void RemoveSessionHandle(MsgRemoveSession msg)
        {
            _sessions.Remove(msg.Session.SessionID);
            Log.Logger.Information($"[SessionManager] Remove Session Comp Session Count : {GetAllSessions().Count()}");
        }

        // 모든 세션 조회
        private void GetAllSessionsHandle()
        {
            var allSessions = GetAllSessions();
            Sender.Tell(allSessions); // 세션 목록 반환
        }
        private List<ClientSession> GetAllSessions()
        {
            var allSessions = _sessions.Values.ToList();
            return allSessions;
        }
        private void FlushAllSessions()
        {
            foreach (var session in _sessions.Values)
            {
                session.FlushSend();
            }
        }
    }
}
