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
                _roomManager.Tell(new RoomManagerActor.AddClientMessage(this));
            }
        }
    }
}
