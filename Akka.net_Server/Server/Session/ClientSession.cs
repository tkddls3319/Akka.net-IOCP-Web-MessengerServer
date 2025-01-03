using Akka.Actor;

using Google.Protobuf;
using Google.Protobuf.Protocol;

using ServerCore;

using System.Net;

namespace Server
{
    public class ClientSession : PacketSession
    {
        public IActorRef Room { get; set; }
        static IActorRef _roomManager;

        public int SessionID { get; set; }
        List<ArraySegment<byte>> _reservSendList = new List<ArraySegment<byte>>();
        object _lock = new object();

        long _lastSendTick = 0;
        int _reservedSendBytes = 0;
        public void Send(IMessage packet)
        {
            string packetName = packet.Descriptor.Name.Replace("_", string.Empty);
            PacketID packetID = (PacketID)Enum.Parse(typeof(PacketID), packetName);

            ushort size = (ushort)packet.CalculateSize();
            byte[] sendBuffer = new byte[size + 4];

            Array.Copy(BitConverter.GetBytes((ushort)(size + 4)), 0, sendBuffer, 0, sizeof(ushort));
            Array.Copy(BitConverter.GetBytes((ushort)(packetID)), 0, sendBuffer, sizeof(ushort), sizeof(ushort));
            Array.Copy(packet.ToByteArray(), 0, sendBuffer, 4, size);


            Console.WriteLine($"SEND - Packet : {packet.Descriptor.Name}, SessionId : {SessionID}");
            lock (_lock)
            {
                _reservSendList.Add(new ArraySegment<byte>(sendBuffer));
                _reservedSendBytes += sendBuffer.Length;
            }

        }
        public ClientSession()
        {
            if (_roomManager == null)
            {
                _roomManager = Program.ServerActorSystem.ActorSelection("/user/RoomManagerActor")
                                               .ResolveOne(TimeSpan.FromSeconds(1))
                                               .Result;
            }
        }
        public void FlushSend()
        {
            List<ArraySegment<byte>> sendList = null;

            lock (_lock)
            {
                long tick = Environment.TickCount64 - _lastSendTick;

                if (tick < 100 && _reservedSendBytes < 100000)
                    return;

                _reservedSendBytes = 0;
                _lastSendTick = Environment.TickCount64;

                sendList = _reservSendList;
                _reservSendList = new List<ArraySegment<byte>>();

            }
            Send(sendList);
        }
        public override void OnConnected(EndPoint endPoint)
        {
            if (Room == null)
            {
                _roomManager.Tell(new RoomManagerActor.AddClient(this));
                Console.WriteLine($"OnConnected - IP : {endPoint}, SessionId : {SessionID}");
            }
        }
        public override void OnDisconnected(EndPoint endPoint)
        {
            if (Room != null)
            {
                Room.Tell(new RoomManagerActor.RemoveRoom(SessionID));
                Console.WriteLine($"{SessionID} Disconnected");
            }
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
