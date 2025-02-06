using Akka.Actor;
using Google.Protobuf.Protocol;

using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Akka.Server
{
    public partial class ClientSession : PacketSession
    {
        public void EnterServerHandler(C_EnterServer enterPacket)
        {
            AccountName = enterPacket.Client.AccountName;
            if (Room == null)
            {
                RoomManager.Tell(new RoomManagerActor.AddClientCommand(this, enterPacket.Client.RoomID));
            }
        }
        public void NewRoomHandler(C_NewRoomAndEnterServer packet)
        {
            AccountName = packet.Client.AccountName;
            if (Room == null)
            {
                RoomManager.Tell(new RoomManagerActor.CreateRoomAndAddClientCommand(this));
            }
        }
        public void LeaveRoomHandler()
        {
            if (Room != null)
            {
                Room.Tell(new RoomActor.LeaveClientCommand(SessionID, false));
            }
        }
    }
}
