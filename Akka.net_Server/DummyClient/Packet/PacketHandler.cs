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

        Console.WriteLine($"\t\t\t{s_chat.ObjectId}번 유저");
        Console.WriteLine($"\t\t\tChat : {s_chat.Chat}");
    }
    public static void S_EnterServerHandler(PacketSession session, IMessage packet)
    {
        ServerSession clientSession = (ServerSession)session;
        S_EnterServer s_enter = (S_EnterServer)packet;

        clientSession.MakeInputThread();

        Console.WriteLine($"> {s_enter.Client.RoomID}번 채팅 방에 참여했습니다.");
        Console.WriteLine($"> 당신의 아이디는 {s_enter.Client.ObjectId}입니다. 현재 참여인원 {s_enter.Client.ClientCount}명.");
        Console.WriteLine("> 메시지를 입력하세요.");
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

        string ids = string.Empty;
        foreach (var id in s_spawn.ObjectIds)
        {
            ids += id + " ";
        }
        if (string.IsNullOrEmpty(ids) == false)
            Console.WriteLine($"> {ids}님이 채팅 방에 참여했습니다. 현재 참여인원 {s_spawn.ClientCount}명.");
    }
    public static void S_DespawnHandler(PacketSession session, IMessage packet)
    {
        ServerSession clientSession = (ServerSession)session;
        S_Despawn s_dspawn = (S_Despawn)packet;

        Console.WriteLine($"> {s_dspawn.ObjectId}님이 채팅 방에서 나가셨습니다.. 현재 참여인원 {s_dspawn.ClientCount}명.");
    }
}
