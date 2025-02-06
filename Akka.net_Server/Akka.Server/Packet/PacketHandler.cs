using Akka.Actor;

using Google.Protobuf;
using Google.Protobuf.Protocol;
using Akka.Server;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using ServerCore;
using System.Security.Cryptography.X509Certificates;
using System.Net.Sockets;

public class PacketHandler
{
    public static void C_EnterServerHandler(PacketSession session, IMessage packet)
    {
        ClientSession clientSession = (ClientSession)session;
        C_EnterServer enterPacket = (C_EnterServer)packet;

        clientSession.EnterServerHandler(enterPacket);
    }
    public static void C_ChatHandler(PacketSession session, IMessage packet)
    {
        ClientSession clientSession = (ClientSession)session;
        C_Chat c_chat = (C_Chat)packet;

        if (clientSession == null)
            return;

        IActorRef room = clientSession.Room;

        if (room == null)
            return;

        room.Tell(new MessageCustomCommand<ClientSession, C_Chat>(clientSession, c_chat));
    }

    public static void C_NewRoomAndEnterServerHandler(PacketSession session, IMessage packet)
    {
        ClientSession clientSession = (ClientSession)session;
        C_NewRoomAndEnterServer newroom = (C_NewRoomAndEnterServer)packet;

        if (clientSession == null)
            return;

        clientSession.NewRoomHandler(newroom);
    }

    internal static void C_LeaveRoomHandler(PacketSession session, IMessage packet)
    {
        ClientSession clientSession = (ClientSession)session;
        C_LeaveRoom newroom = (C_LeaveRoom)packet;

        if (clientSession == null)
            return;

        clientSession.LeaveRoomHandler();
    }

    internal static void C_MultiTestRoomHandler(PacketSession session, IMessage packet)
    {
        ClientSession clientSession = (ClientSession)session;
        clientSession.AccountName = "";
        if (clientSession.Room == null)
        {
            clientSession.RoomManager.Tell(new RoomManagerActor.MultiTestRoomCommand(clientSession));
        }
    }
}
