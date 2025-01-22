﻿using Akka.IO;

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

        string message = s_chat.Chat.TrimEnd('\n', ' ');

        Util.PrintDisplayMessage(s_chat.AccountName, message, s_chat.Time.ToDateTime().ToString("MM-dd HH:mm:ss"));
        //Console.WriteLine($"\t\t\t[{s_chat.AccountName}]");
        //Console.WriteLine($"\t\t\t[{s_chat.Time.ToDateTime().ToString("MM-dd HH:mm:ss")} ▶  {message}]\n");
    }
    public static void S_EnterServerHandler(PacketSession session, IMessage packet)
    {
        ServerSession clientSession = (ServerSession)session;
        S_EnterServer s_enter = (S_EnterServer)packet;

        clientSession.MakeInputThread();

        Console.WriteLine($"> {s_enter.Client.RoomID}번 채팅 방에 참여했습니다.");
        Console.WriteLine($"> 당신의 아이디는 {s_enter.Client.ObjectId}입니다. 현재 참여인원 {s_enter.Client.ClientCount}명.");

        if(s_enter.Client.ClientCount == 1)
        {
            Console.WriteLine("");
            Console.WriteLine("※ 채팅방에 혼자 있습니다. 채팅을 위해 DummyClient를 하나 더 켜주세요.");
            Console.WriteLine("※ DummyClient.exe경로 : DummyClient -> bin->Debug or Release -> DummyClient.exe");
            Console.WriteLine("");
        }
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

        string names = string.Empty;
        foreach (var name in s_spawn.AccountNames)
        {
            names += name + " ";
        }
        if (string.IsNullOrEmpty(names) == false)
            Console.WriteLine($"> {names}님이 채팅 방에 참여했습니다. 현재 참여인원 {s_spawn.ClientCount}명.");
    }
    public static void S_DespawnHandler(PacketSession session, IMessage packet)
    {
        ServerSession clientSession = (ServerSession)session;
        S_Despawn s_dspawn = (S_Despawn)packet;

        Console.WriteLine($"> {s_dspawn.AccountName}님이 채팅 방에서 나가셨습니다.. 현재 참여인원 {s_dspawn.ClientCount}명.");
    }
}
