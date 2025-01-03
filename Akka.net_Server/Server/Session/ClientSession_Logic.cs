using Akka.Actor;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public partial class ClientSession : PacketSession
    {
        public void EnterServerHandler()
        {
            if (Room == null)
            {
                _roomManager.Tell(new RoomManagerActor.AddClient(this));
            }
        }
    }
}
