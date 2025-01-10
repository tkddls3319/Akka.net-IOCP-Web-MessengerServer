using Akka.Actor;

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
        public class FlushSendAll { }

        // 타이머 키와 메시지
        private sealed class TimerKey
        {
            public static readonly TimerKey Instance = new();
            private TimerKey() { }
        }
        private sealed class TimerMessage
        {
            public static readonly TimerMessage Instance = new();
            private TimerMessage() { }
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
            Receive<SetRoomManagerActor>(msg => _roomManager = msg.RoomManager);

            Receive<GenerateSession>(msg => GenerateSessionHandle(msg));
            Receive<FindSession>(msg => FindSessionHandle(msg));
            Receive<RemoveSession>(msg => RemoveSessionHandle(msg));
            Receive<GetAllSessions>(_ => GetAllSessionsHandle());

            // 초기화: 타이머 시작
            Timers.StartPeriodicTimer(
                key: TimerKey.Instance,
                msg: TimerMessage.Instance,
                initialDelay: TimeSpan.FromMilliseconds(100),  // 초기 지연 시간
                interval: TimeSpan.FromMilliseconds(100));    // 주기적 실행 간격

            // 타이머 메시지 처리
            Receive<TimerMessage>(_ => FlushAllSessions());
        }

        // 세션 생성
        private void GenerateSessionHandle(GenerateSession msg)
        {
            var clientSession = new ClientSession(_roomManager);
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
            _sessions.Remove(msg.Session.SessionID);
        }

        // 모든 세션 조회
        private void GetAllSessionsHandle()
        {
            var allSessions = _sessions.Values.ToList();
            Sender.Tell(allSessions); // 세션 목록 반환
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
