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
        public class SetRoomManagerActorMessage
        {
            public IActorRef RoomManager { get; }
            public SetRoomManagerActorMessage(IActorRef roomManager) => RoomManager = roomManager;
        }
        public class GenerateSessionMessage
        {
            public Socket SessionSocket { get; set; }
            public GenerateSessionMessage(Socket sessionSocekt)
            {
                SessionSocket = sessionSocekt;
            }
        }
        public class FindSessionMessage
        {
            public int SessionID { get; }
            public FindSessionMessage(int sessionID) => SessionID = sessionID;
        }
        public class RemoveSessionMessage
        {
            public ClientSession Session { get; }
            public RemoveSessionMessage(ClientSession session) => Session = session;
        }
        public class GetAllSessionsMessage { }
        public class FlushSendAllMessage { }

        // 타이머 키와 메시지
        private sealed class TimerKeyMessage
        {
            public static readonly TimerKeyMessage Instance = new();
            private TimerKeyMessage() { }
        }
        private sealed class TimerMessageMessage
        {
            public static readonly TimerMessageMessage Instance = new();
            private TimerMessageMessage() { }
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
            //종속성
            Receive<SetRoomManagerActorMessage>(msg => _roomManager = msg.RoomManager);

            // 세션 생성
            Receive<GenerateSessionMessage>(msg =>
            {
                var clientSession = new ClientSession(_roomManager);
                clientSession.SessionID = ++_sessionID;
                _sessions.Add(clientSession.SessionID, clientSession);

                clientSession.Start(msg.SessionSocket);
                clientSession.OnConnected(msg.SessionSocket.RemoteEndPoint);

                Log.Logger.Information($"[SessionManager] Generate Session Comp Session Count : {GetAllSessions().Count()}");

            });
            // 세션 조회
            Receive<FindSessionMessage>(msg =>
            {
                _sessions.TryGetValue(msg.SessionID, out var clientSession);
                Sender.Tell(clientSession); // 결과 반환
            });
            // 세션 제거
            Receive<RemoveSessionMessage>(msg =>
            {
                _sessions.Remove(msg.Session.SessionID);
                Log.Logger.Information($"[SessionManager] Remove Session Comp Session Count : {GetAllSessions().Count()}");
            });
            // 모든 세션 조회
            Receive<GetAllSessionsMessage>(_ =>
            {
                var allSessions = GetAllSessions();
                Sender.Tell(allSessions); // 세션 목록 반환
            });

            //Session에 Send를 비워주는 timer
            {
                Timers.StartPeriodicTimer(
                    key: TimerKeyMessage.Instance,
                    msg: TimerMessageMessage.Instance,
                    initialDelay: TimeSpan.FromMilliseconds(100),  // 초기 지연 시간
                    interval: TimeSpan.FromMilliseconds(100));    // 주기적 실행 간격
                Receive<TimerMessageMessage>(_ => FlushAllSessions());
            }
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
