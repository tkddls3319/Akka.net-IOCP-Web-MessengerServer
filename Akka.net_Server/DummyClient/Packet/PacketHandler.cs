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


        Console.WriteLine(s_chat.Chat);
    }
    public static void S_EnterServerHandler(PacketSession session, IMessage packet)
    {
        ServerSession clientSession = (ServerSession)session;
        S_EnterServer s_enter = (S_EnterServer)packet;

        C_Chat chat_ = new C_Chat()
        {
            Chat = "클라이언트 접속1 클라이언트 접속2 클라이언트 접속3 클라이언트 접속4 클라이언트 접속5 클라이언트 접속6\n"
        };
        clientSession.Send(chat_);
        Console.WriteLine($"S_EnterServerHandler{s_enter.Client.ObjectId}");
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
