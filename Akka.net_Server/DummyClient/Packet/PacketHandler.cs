using DummyClient;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

public class PacketHandler
{
    public static void S_ChatHandler(PacketSession session, IMessage packet)
    {
        ServerSession clientSession = (ServerSession)session;
        S_Chat s_chat = (S_Chat)packet;

        Console.WriteLine($"S_ChatHandler - ObjectID : {s_chat.ObjectId} Chat : {s_chat.Chat}");
    }
    public static void S_EnterServerHandler(PacketSession session, IMessage packet)
    {
        ServerSession clientSession = (ServerSession)session;
        S_EnterServer s_enter = (S_EnterServer)packet;

        clientSession.MakeInputThread();

        Console.WriteLine($"S_EnterServerHandler - Your Object ID : {s_enter.Client.ObjectId}");
    }

    public static void S_LeaveServerHandler(PacketSession session, IMessage packet)
    {
        ServerSession clientSession = (ServerSession)session;
        S_LeaveServer s_leave = (S_LeaveServer)packet;
    }
    public static void S_SpawnHandler(PacketSession session, IMessage packet)
    {
        ServerSession clientSession = (ServerSession)session;
        S_Spawn s_spawn = (S_Spawn)packet;

        //Console.WriteLine(s_spawn);
    }
    public static void S_DespawnHandler(PacketSession session, IMessage packet)
    {
        ServerSession clientSession = (ServerSession)session;
        S_Despawn s_dspawn = (S_Despawn)packet;

        //Console.WriteLine(s_dspawn.ObjectIds);
    }
}
