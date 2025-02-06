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
        public record SetRoomManagerActorCommand(IActorRef RoomManager);

        public record GenerateSessionCommand(Socket SessionSocket);

        public record FindSessionQuery(int SessionID);

        public record RemoveSessionCommand(ClientSession Session);

        public record GetAllSessionsQuery;

        // 타이머 키와 메시지
        public sealed record TimerKeyCommand
        {
            public static readonly TimerKeyCommand Instance = new();
            private TimerKeyCommand() { }
        }

        public sealed record TimerMessageCommand
        {
            public static readonly TimerMessageCommand Instance = new();
            private TimerMessageCommand() { }
        }
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
            Receive<SetRoomManagerActorCommand>(msg => _roomManager = msg.RoomManager);

            // 세션 생성
            Receive<GenerateSessionCommand>(msg =>
            {
                var clientSession = new ClientSession(_roomManager);
                clientSession.SessionID = ++_sessionID;
                _sessions.Add(clientSession.SessionID, clientSession);

                clientSession.Start(msg.SessionSocket);
                clientSession.OnConnected(msg.SessionSocket.RemoteEndPoint);

                Log.Logger.Information($"[SessionManager] Generate Session Comp Session Count : {GetAllSessions().Count()}");

            });
            // 세션 조회
            Receive<FindSessionQuery>(msg =>
            {
                _sessions.TryGetValue(msg.SessionID, out var clientSession);
                Sender.Tell(clientSession); // 결과 반환
            });
            // 세션 제거
            Receive<RemoveSessionCommand>(msg =>
            {
                _sessions.Remove(msg.Session.SessionID);
                Log.Logger.Information($"[SessionManager] Remove Session Comp Session Count : {GetAllSessions().Count()}");
            });
            // 모든 세션 조회
            Receive<GetAllSessionsQuery>(_ =>
            {
                var allSessions = GetAllSessions();
                Sender.Tell(allSessions); // 세션 목록 반환
            });

            //Session에 Send를 비워주는 timer
            {
                Timers.StartPeriodicTimer(
                    key: TimerKeyCommand.Instance,
                    msg: TimerMessageCommand.Instance,
                    initialDelay: TimeSpan.FromMilliseconds(1),  // 초기 지연 시간
                    interval: TimeSpan.FromMilliseconds(1));    // 주기적 실행 간격
                Receive<TimerMessageCommand>(_ => FlushAllSessions());
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
