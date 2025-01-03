using Akka.Actor;

using Google.Protobuf.Protocol;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class RoomActor : ReceiveActor
    {
        #region Message
        public class GetClientCount { }
        public class EnterClient
        {
            public ClientSession Session { get; }
            public EnterClient(ClientSession session)
            {
                Session = session;
            }
        }
        public class LeaveClient
        {
            public int ClientId { get; set; }
            public LeaveClient(int clientId)
            {
                ClientId = clientId;
            }
        }
        #endregion

        public int RoomID { get; set; }
        Dictionary<int, ClientSession> _clients = new Dictionary<int, ClientSession>();

        public RoomActor(int roomNumber)
        {
            RoomID = roomNumber;

            Receive<EnterClient>(msg => EnterClientHandler(msg));
            Receive<GetClientCount>(_ => Sender.Tell(_clients.Count));
            Receive<LeaveClient>(msg => LeaveClientHandler(msg.ClientId));
        }
        protected override void PreStart()
        {
            Console.WriteLine($"Room {RoomID} started.");
        }
        protected override void PostStop()
        {
            Console.WriteLine($"Room {RoomID} stopped.");
        }
        private void EnterClientHandler(EnterClient client)
        {
            if (client.Session == null)
                return;

            ClientSession session = client.Session;
            if (_clients.ContainsKey(session.SessionID))
                return;

            _clients[session.SessionID] = session;
            session.Room = Self;
            Console.WriteLine($"Client with SessionID {session.SessionID} added to Room {RoomID}.");
        }
        private void LeaveClientHandler(int clientId)
        {
            ClientSession client = null;
            if (_clients.Remove(clientId, out client) == false)
                return;

            

            client.Room = null;

            Console.WriteLine($"Client Leave {clientId}");
            //{
            //    S_LeaveServer leavePacket = new S_LeaveServer();
            //    client.Send(leavePacket);
            //}

            //타인에게 전달
            //{
            //    S_Despawn despawnPacket = new S_Despawn();
            //    despawnPacket.ObjectIds.Add(clientId);

            //    foreach (ClientSession p in _clients.Values)
            //    {
            //        if (p.SessionID != clientId)
            //            p.Send(despawnPacket);
            //    }
            //}
        }
    }

}
