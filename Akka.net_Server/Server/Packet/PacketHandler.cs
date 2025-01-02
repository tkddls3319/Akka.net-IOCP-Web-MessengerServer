using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

public class PacketHandler
{
    public static void C_EnterServerHandler(PacketSession session, IMessage packet)
    {
        //ClientSession clientSession = (ClientSession)session;
        //C_EnterServer enterPacket = (C_EnterServer)packet;

        //clientSession.EnterHandle(enterPacket);
        //Console.WriteLine("C_EnterServerHandler");
    }
    public static void C_ChatHandler(PacketSession session, IMessage packet)
    {
        //ClientSession clientSession = (ClientSession)session;
        //C_Chat c_chat = (C_Chat)packet;

        //ClientObject client = clientSession.MyClient;
        //if (client == null)
        //    return;

        //MsgRoom room = client.MyRoom;

        //if (room == null)
        //    return;

        //room.Push(room.HandleChat, client, c_chat);

    }
}
