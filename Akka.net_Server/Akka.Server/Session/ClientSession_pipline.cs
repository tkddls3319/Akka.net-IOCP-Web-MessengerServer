using Akka.Actor;
using Google.Protobuf;
using Google.Protobuf.Protocol;

using Serilog;

using ServerCore;
using System.Net;

using static Akka.IO.UdpConnected;

namespace Akka.Server
{
    public class ClientSession_pipline : PacketSession
    {

        public IActorRef Room { get; set; }
        public IActorRef RoomManager;
        public IActorRef SessionManager;
        public int SessionID { get; set; }
        public string AccountName { get; set; }

        // 예약 송신 리스트: 이제 ArraySegment<byte> 대신 ReadOnlyMemory<byte> 사용 :contentReference[oaicite:3]{index=3}
        private List<ReadOnlyMemory<byte>> _reservSendList = new List<ReadOnlyMemory<byte>>();
        private readonly object _lock = new object();

        private long _lastSendTick = 0;
        private int _reservedSendBytes = 0;

        public ClientSession_pipline(IActorRef sessionManager, IActorRef roomManager)
        {
            SessionManager = sessionManager;
            RoomManager = roomManager;
        }

        /// <summary>
        /// protobuf IMessage를 길이 + 패킷ID + 바디 구조로 직렬화해서 예약 큐에 넣는다.
        /// [size(2) | id(2) | body(...)]
        /// </summary>
        public void Send(IMessage packet)
        {
            string packetName = packet.Descriptor.Name.Replace("_", string.Empty);
            PacketID packetID = (PacketID)Enum.Parse(typeof(PacketID), packetName);

            ushort bodySize = (ushort)packet.CalculateSize();
            ushort totalSize = (ushort)(bodySize + 4); // size(2) + id(2) + body

            byte[] sendBuffer = new byte[totalSize];
            var span = sendBuffer.AsSpan();

            // [0..2): 전체 길이
            BitConverter.TryWriteBytes(span.Slice(0, sizeof(ushort)), totalSize);

            // [2..4): PacketID
            BitConverter.TryWriteBytes(span.Slice(sizeof(ushort), sizeof(ushort)), (ushort)packetID);

            // [4..): protobuf payload
            // protobuf 버전에 따라 CodedOutputStream 생성자가 다를 수 있으니,
            // 필요시 (byte[], offset, length) 생성자를 사용해도 됩니다.
            using (var cos = new CodedOutputStream(sendBuffer, 4, bodySize))
            {
                packet.WriteTo(cos);
                cos.Flush();
            }

            lock (_lock)
            {
                _reservSendList.Add(sendBuffer.AsMemory());
                _reservedSendBytes += sendBuffer.Length;
            }
        }

        /// <summary>
        /// Tick/크기 조건을 만족할 때 예약된 패킷들을 실제 세션 송신 큐로 밀어 넣는다.
        /// </summary>
        public void FlushSend()
        {
            List<ReadOnlyMemory<byte>>? sendList = null;

            lock (_lock)
            {
                long tick = Environment.TickCount64 - _lastSendTick;

                if (tick < 100 && _reservedSendBytes < 100_000)
                    return;

                _reservedSendBytes = 0;
                _lastSendTick = Environment.TickCount64;

                sendList = _reservSendList;
                _reservSendList = new List<ReadOnlyMemory<byte>>();
            }

            // Session.Send(ReadOnlyMemory<byte>) 호출 → Channel → SendLoop → Socket.SendAsync
            if (sendList != null && sendList.Count > 0)
            {
                foreach (var buffer in sendList)
                {
                    Send(buffer);
                }
            }
        }

        public override void OnConnected(EndPoint endPoint)
        {
            Log.Logger.Information($"[ClientSession] OnConnected - IP : {endPoint}, SessionId : {SessionID}");
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            if (Room != null)
            {
                Room.Tell(new RoomActor.LeaveClientCommand(SessionID, true));
                Log.Logger.Information($"[ClientSession] Disconnected - IP : {endPoint}, SessionId : {SessionID}");
            }

            SessionManager.Tell(new SessionManagerActor.RemoveSessionCommand(this));
        }

        public override void OnRecvedPacket(ArraySegment<byte> buffer)
        {
            // TODO: PacketManager도 ReadOnlyMemory/Span 기반으로 리팩토링하면 더 좋음
            PacketManager.Instance.OnRecvPacket(this, buffer);
        }

        public override void OnSended(int numOfByte)
        {
            // 필요하면 전송량 로깅/모니터링 가능
        }
    }
}