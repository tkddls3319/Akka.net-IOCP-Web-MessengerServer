using Google.Protobuf;
using Google.Protobuf.Protocol;

using ServerCore;

using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace DummyClient
{
    public class ServerSession : PacketSession
    {
        public int SessionId { get; set; }  
        public void Send(IMessage packet)
        {
            string packetName = packet.Descriptor.Name.Replace("_", string.Empty);
            PacketID packetID = (PacketID)Enum.Parse(typeof(PacketID), packetName);

            ushort size = (ushort)packet.CalculateSize();
            byte[] sendBuffer = new byte[size + 4];

            Array.Copy(BitConverter.GetBytes((ushort)(size + 4)), 0, sendBuffer, 0, sizeof(ushort));
            Array.Copy(BitConverter.GetBytes((ushort)(packetID)), 0, sendBuffer, sizeof(ushort), sizeof(ushort));
            Array.Copy(packet.ToByteArray(), 0, sendBuffer, 4, size);

            Send(new ArraySegment<byte>(sendBuffer));
        }

        public void MakeInputThread()
        {
            if (Program.IsMultitest)
                return;

            Thread t1 = new Thread(() =>
                {
                    while (true)
                    {
                        if (Program.RoomEnter)
                        {
                            Console.Write(">> ");
                            string input = Console.ReadLine();

                            if (input == "ESC")
                            {
                                Send(new C_LeaveRoom());
                            }
                            else
                            {
                                Util.AddOrPrintDisplayMessage(input, Program.AccountName, DateTime.Now.ToString());
                                C_Chat packet = new C_Chat();
                                packet.Chat = input;
                                Send(packet);
                            }
                        }
                        Thread.Sleep(500);
                    }
                });

            t1.Name = "InputThread";
            t1.Start();
        }

        public override void OnConnected(EndPoint endPoint)
        {
            if (Program.IsMultitest)
            {
                Send(new C_MultiTestRoom());
                return;
            }

            Util.AddOrPrintDisplayMessage("==========Server Connected==========");
            Util.AddOrPrintDisplayMessage($"Server EndPoint - {endPoint}");

            MakeInputThread();
            RoomChoice();
        }
        public override void OnDisconnected(EndPoint endPoint)
        {   
            Util.AddOrPrintDisplayMessage("[Session] Server Disconnected....");
        }

        public override void OnRecvedPacket(ArraySegment<byte> buffer)
        {
            PacketManager.Instance.OnRecvPacket(this, buffer);
        }
        public override void OnSended(int numOfByte)
        {
        }

        public void RoomChoice()
        {
            int? roomId = Util.RoomChoice(Program.RoomInfos);

            //새로운 방 생성
            if (roomId == null)
            {
                Send(new C_NewRoomAndEnterServer()
                {
                    Client = new ClientInfo()
                    {
                        AccountName = Program.AccountName,
                    }
                });
            }
            else
            {
                Send(new C_EnterServer()
                {
                    Client = new ClientInfo()
                    {
                        AccountName = Program.AccountName,
                        RoomID = roomId.Value,
                    }
                });
            }
        }
    }
}
