using Akka.Util;

using Google.Protobuf;
using Google.Protobuf.Protocol;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogServer
{
    public static class MessageHelper
    {
        public static byte[] SerializeWithType<T>(T packet) where T : IMessage
        {
            string packetName = packet.Descriptor.Name.Replace("_", string.Empty);
            PacketID packetID = (PacketID)Enum.Parse(typeof(PacketID), packetName);

            ushort size = (ushort)packet.CalculateSize();
            byte[] sendBuffer = new byte[size + 2];

            Array.Copy(BitConverter.GetBytes((ushort)(packetID)), 0, sendBuffer, 0, sizeof(ushort));
            Array.Copy(packet.ToByteArray(), 0, sendBuffer, sizeof(ushort), size);

            return sendBuffer;
        }
        public static IMessage DeserializeWithType(byte[] data)
        {
            ushort id = BitConverter.ToUInt16(data);

            switch ((PacketID)id)
            {
                case PacketID.CChat:
                    var message = new C_Chat();
                    byte[] newByte = data.Skip(2).ToArray();
                    message.MergeFrom(newByte);

                    return message;
            }

            return null;
        }
    }
}
