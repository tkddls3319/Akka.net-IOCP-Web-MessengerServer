using Akka.Event;
using Akka.IO;

using DummyClient;
using DummyClient.Session;

using Google.Protobuf;
using Google.Protobuf.ClusterProtocol;
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

        if (Program.IsMultitest)
        {
            Console.WriteLine($">{s_chat.Time.ToDateTime().ToString("MM-dd HH:mm:ss")} [{s_chat.AccountName}{s_chat.ObjectId}번] {s_chat.Chat.Trim()}");
            return;
        }
        string message = s_chat.Chat.TrimEnd('\n', ' ');
        Util.AddOrPrintDisplayMessage( message, s_chat.AccountName, s_chat.Time.ToDateTime().ToString("MM-dd HH:mm:ss"));
    }
    public static void S_EnterServerHandler(PacketSession session, IMessage packet)
    {
        ServerSession clientSession = (ServerSession)session;
        S_EnterServer s_enter = (S_EnterServer)packet;

        clientSession.IsRoomEnter = true;
        clientSession.SessionId = s_enter.Client.ObjectId;

        if (Program.IsMultitest)
        {
            return;
        }
        
        Util.AddDisplayMessage($"> {s_enter.Client.RoomID}번 채팅 방에 참여했습니다.");
        Util.AddDisplayMessage($"> 당신의 아이디는 {s_enter.Client.ObjectId}입니다. 현재 참여인원 {s_enter.Client.ClientCount}명.");

        if (s_enter.Client.ClientCount == 1)
        {
            Util.AddDisplayMessage("");
            Util.AddDisplayMessage("※ 채팅방에 혼자 있습니다. 채팅을 위해 DummyClient를 하나 더 켜주세요.");
            Util.AddDisplayMessage("※ DummyClient.exe경로 : DummyClient -> bin->Debug or Release -> DummyClient.exe");
            Util.AddDisplayMessage("");
        }
        Util.AddDisplayMessage($"> ※※※ 채팅방에서 나가고 싶으시면 ESC를 채팅방에 입력 후 ENTER키를 누르세요. ※※※");
        Util.AddOrPrintDisplayMessage("> 메시지를 입력하세요.");
    }

    public static void S_LeaveServerHandler(PacketSession session, IMessage packet)
    {
        ServerSession clientSession = (ServerSession)session;
        S_LeaveServer s_leave = (S_LeaveServer)packet;

        clientSession.IsRoomEnter = false;

        //채팅방 다시 선택
        Task.Run(async () =>
        {
            var result = await WebManager.Instance.SendPostRequest<GetRoomsAccountPacketRes>(
                "account/getrooms",
                new GetRoomsAccountPacketReq()
            );

            Program.RoomInfos = result.RoomList;
            clientSession.RoomChoice();
        });

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
            Util.AddOrPrintDisplayMessage($"> {names}님이 채팅 방에 참여했습니다. 현재 참여인원 {s_spawn.ClientCount}명.");
    }
    public static void S_DespawnHandler(PacketSession session, IMessage packet)
    {
        ServerSession clientSession = (ServerSession)session;
        S_Despawn s_dspawn = (S_Despawn)packet;

        Util.AddOrPrintDisplayMessage($"> {s_dspawn.AccountName}님이 채팅 방에서 나가셨습니다.. 현재 참여인원 {s_dspawn.ClientCount}명.");
    }
}
