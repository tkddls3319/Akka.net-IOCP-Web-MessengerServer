using Akka.Actor;

using Google.Protobuf;
using Google.Protobuf.ClusterProtocol;
using Google.Protobuf.Protocol;

using Serilog;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Akka.LogServer
{
    public class ChatLogWriteActor : ReceiveActor, IWithTimers
    {
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

        public ITimerScheduler Timers { get; set; } = null!;

        private readonly Dictionary<int, List<WriteMessageCommand<SL_ChatWriteLogCommand>>> _roomMessageBuffer = new(); // RoomId 별 메시지 저장
        private readonly int _maxMessageCount = 10; // 최대 메시지 개수 (초과 시 즉시 기록)
        private readonly ICancelable _flushTimer;
        protected override void PostStop()
        {
            _flushTimer.Cancel(); // Actor가 종료될 때 타이머 중지
            base.PostStop();
        }

        public ChatLogWriteActor()
        {
            Receive<WriteMessageCommand<SL_ChatWriteLogCommand>>(message =>
            {
                //SL_ChatWriteLogCommand writeLog = message.Message;

                ////TODO:
                //var logger = SeriLogManager.Get($"Room{writeLog.Chat.RoomId}");
                //logger?.Information($"{message.Sender} - {writeLog.Chat}");

                var writeLog = message.Message;
                var roomId = writeLog.Chat.RoomId;

                if (!_roomMessageBuffer.ContainsKey(roomId))
                {
                    _roomMessageBuffer[roomId] = new List<WriteMessageCommand<SL_ChatWriteLogCommand>>();
                }

                _roomMessageBuffer[roomId].Add(message);

                // 최대 메시지 개수 초과 시 즉시 로깅 후 초기화
                if (_roomMessageBuffer[roomId].Count >= _maxMessageCount)
                {
                    FlushLogs(roomId);
                }
            });

            {
                Timers.StartPeriodicTimer(
                    key: TimerKeyCommand.Instance,
                    msg: TimerMessageCommand.Instance,
                    initialDelay: TimeSpan.FromMilliseconds(100),  // 초기 지연 시간
                    interval: TimeSpan.FromMilliseconds(1000));    // 주기적 실행 간격
                Receive<TimerMessageCommand>(_ => FlushAllLogs());
            }
        }
        private void FlushLogs(int roomId)
        {
            if (_roomMessageBuffer.TryGetValue(roomId, out var messages) && messages.Count > 0)
            {
                var logger = SeriLogManager.Get($"Room{roomId}");
                StringBuilder stringBuilder = new StringBuilder();
                foreach (var log in messages)
                {
                    SL_ChatWriteLogCommand writeLog = log.Message;
                    stringBuilder.Append($"{log.Sender} - {writeLog.Chat}");
                }
                logger?.Information(stringBuilder.ToString());

                _roomMessageBuffer[roomId].Clear(); // 로그 기록 후 초기화
            }
        }
        private void FlushAllLogs()
        {
            foreach (var roomId in _roomMessageBuffer.Keys.ToList()) // 키 리스트를 복사해서 안전하게 반복
            {
                FlushLogs(roomId);
            }
        }
    }
}
