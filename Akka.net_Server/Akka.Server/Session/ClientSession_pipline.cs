
using Akka.Actor;

using Google.Protobuf;
using Google.Protobuf.Protocol;

using Serilog;

using ServerCore;

using System.Net;

namespace Akka.Server
{
    public partial class ClientSession_pipline : PacketSession_pipline
    {
        public IActorRef Room { get; set; }
        public IActorRef RoomManager;
        public IActorRef SessionManager;
        public int SessionID { get; set; }
        public string AccountName { get; set; }

        // 예약 송신 리스트
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
        /// protobuf IMessage → [size(2) | id(2) | body] 직렬화 후 큐에 넣는다.
        /// </summary>
        public void EnqueuePacket(IMessage packet)
        {
            string packetName = packet.Descriptor.Name.Replace("_", string.Empty);
            PacketID packetID = (PacketID)Enum.Parse(typeof(PacketID), packetName);

            ushort bodySize = (ushort)packet.CalculateSize();
            ushort totalSize = (ushort)(bodySize + 4);

            byte[] sendBuffer = new byte[totalSize];
            var span = sendBuffer.AsSpan();

            // size
            BitConverter.TryWriteBytes(span.Slice(0, 2), totalSize);

            // packet id
            BitConverter.TryWriteBytes(span.Slice(2, 2), (ushort)packetID);

            // protobuf body
            using (var ms = new MemoryStream(sendBuffer, 4, bodySize, writable: true))
            using (var cos = new CodedOutputStream(ms))
            {
                packet.WriteTo(cos);
            }

            lock (_lock)
            {
                _reservSendList.Add(sendBuffer.AsMemory());
                _reservedSendBytes += sendBuffer.Length;
            }
        }

        /// <summary>
        /// 주기적으로 예약된 패킷들을 실제 세션 송신 큐로 밀어 넣는다.
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

            if (sendList != null && sendList.Count > 0)
            {
                foreach (var buffer in sendList)
                {
                    // ★ Session_pipline.Send(ReadOnlyMemory<byte>) 호출
                    Send(buffer);
                }
            }
        }

        public override void OnConnected(EndPoint endPoint)
        {
            Log.Logger.Information($"[ClientSession] Connected : {endPoint}, SessionId : {SessionID}");
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            if (Room != null)
            {
                Room.Tell(new RoomActor.LeaveClientCommand(SessionID, true));
            }

            SessionManager.Tell(new SessionManagerActor.RemoveSessionCommand(this));

            Log.Logger.Information($"[ClientSession] Disconnected : {endPoint}, SessionId : {SessionID}");
        }

        public override void OnRecvedPacket(ArraySegment<byte> buffer)
        {
            PacketManager.Instance.OnRecvPacket(this, buffer);
        }

        public override void OnSended(int numOfByte)
        {
        }
    }
}

