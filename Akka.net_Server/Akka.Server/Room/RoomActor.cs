using Akka.Actor;

using Google.Protobuf;
using Google.Protobuf.ClusterProtocol;
using Google.Protobuf.Protocol;
using Google.Protobuf.WellKnownTypes;

using Serilog;

using ServerCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Akka.Server
{
    public class RoomActor : ReceiveActor
    {
        #region Message
        public class MsgGetClientCount { }
        public class MsgEnterClient
        {
            public ClientSession Session { get; }
            public MsgEnterClient(ClientSession session)
            {
                Session = session;
            }
        }
        public class MsgLeaveClient
        {
            public int ClientId { get; }
            public MsgLeaveClient(int clientId)
            {
                ClientId = clientId;
            }
        }
        #endregion

        #region Actor
        IActorRef _roomManger;
        IActorRef _sessionManager;
        #endregion
        public int RoomID { get; set; }
        Dictionary<int, ClientSession> _clients = new Dictionary<int, ClientSession>();
        int _clinetCount { get { return _clients.Count; } }

        public RoomActor(IActorRef roomMangerActor, IActorRef sessionManagerActor, int roomNumber)
        {
            _roomManger = roomMangerActor;
            _sessionManager = sessionManagerActor;

            RoomID = roomNumber;

            Receive<MsgEnterClient>(msg => EnterClientHandler(msg));
            Receive<MsgGetClientCount>(_ => Sender.Tell(_clients.Count));
            Receive<MsgLeaveClient>(msg => LeaveClientHandler(msg.ClientId));
            Receive<MessageCustom<ClientSession, C_Chat>>(msg => ChatHandle(msg.Item1, msg.Item2));
        }
        private void EnterClientHandler(MsgEnterClient client)
        {
            if (client.Session == null)
                return;

            ClientSession session = client.Session;
            if (_clients.ContainsKey(session.SessionID))
                return;

            _clients[session.SessionID] = session;
            session.Room = Self;

            {
                S_EnterServer enterPaket = new S_EnterServer()
                {
                    Client = new ClientInfo()
                    {
                        ObjectId = session.SessionID,
                        RoomID = RoomID,
                        ClientCount = _clinetCount
                    }
                };

                client.Session.Send(enterPaket);
            }

            //채팅룸안에 모든 사용자 전달
            {
                S_Spawn spawnPacket = new S_Spawn();
                spawnPacket.ClientCount = _clinetCount;
                foreach (ClientSession p in _clients.Values)
                {
                    if (client.Session != p)
                    {
                        spawnPacket.ObjectIds.Add(p.SessionID);
                        spawnPacket.AccountNames.Add(p.AccountName);
                    }
                }
                client.Session.Send(spawnPacket);
            }
            // 타인한테 정보 전송
            {
                S_Spawn spawnPacket = new S_Spawn();
                spawnPacket.ClientCount = _clinetCount;
                spawnPacket.ObjectIds.Add(session.SessionID);
                spawnPacket.AccountNames.Add(session.AccountName);
                foreach (ClientSession p in _clients.Values)
                {
                    if (client.Session != p)
                        p.Send(spawnPacket);
                }
            }

            //이전 모든 채팅을 읽어 새로온 사용자에게 전달
            {
                LS_ChatReadLog response = ClusterActorManager.Instance.GetClusterActor(Define.ClusterType.LogManagerActor)
                    ?.Ask<LS_ChatReadLog>(new SL_ChatReadLog()
                    {
                        RoomId = this.RoomID
                    }, TimeSpan.FromSeconds(3)).Result;

                if (response?.Chats != null)
                {
                    foreach (var chat in response.Chats)
                    {
                        S_Chat readChat = new S_Chat()
                        {
                            Chat = chat.Chat,
                            AccountName = chat.AccoutnName,
                            ObjectId = chat.ObjectId,
                            Time = chat.Time,
                        };

                        client.Session.Send(readChat);
                    }
                }
            }

            Log.Logger.Information($"[Room{RoomID}] Enter Client ID : {session.SessionID}");
        }
        private void LeaveClientHandler(int clientId)
        {
            ClientSession client = null;
            if (_clients.Remove(clientId, out client) == false)
                return;

            client.Room = null;
            _sessionManager.Tell(new SessionManagerActor.MsgRemoveSession(client));

            {
                S_LeaveServer leavePacket = new S_LeaveServer();
                client.Send(leavePacket);
            }

            //타인에게 전달
            {
                S_Despawn despawnPacket = new S_Despawn();
                despawnPacket.ObjectId = clientId;
                despawnPacket.ClientCount = _clinetCount;
                despawnPacket.AccountName = client.AccountName;
                BroadcastExceptSelf(clientId, despawnPacket);
            }

            if (_clients.Count == 0)
                _roomManger.Tell(new RoomManagerActor.MsgRemoveRoom(RoomID));

            Log.Logger.Information($"[Room{RoomID}] Leave Client ID : {clientId}");
        }
        public void ChatHandle(ClientSession player, C_Chat chatPacket)
        {
            if (player == null)
                return;

            int id = player.SessionID;
            string chat = chatPacket.Chat;

            if (string.IsNullOrEmpty(chat))
                return;

            S_Chat severChatPacket = new S_Chat()
            {
                ObjectId = id,
                AccountName = player.AccountName,
                Chat = chat + "\n",
                Time = Timestamp.FromDateTime(DateTime.UtcNow)
            };

            //LogServer클러스터에 Log 전달
            {
                SL_ChatWriteLog logPacket = new()
                {
                    Chat = new ChatObject()
                    {
                        ObjectId = severChatPacket.ObjectId,
                        RoomId = RoomID,
                        Chat = severChatPacket.Chat,
                        Time = severChatPacket.Time,
                        AccoutnName = player.AccountName,
                    }
                };

                ClusterActorManager.Instance.GetClusterActor(Define.ClusterType.LogManagerActor)?.Tell(logPacket);
            }

            BroadcastExceptSelf(id, severChatPacket);
        }

        #region Util
        public void BroadCast(IMessage packet)
        {
            foreach (ClientSession client in _clients.Values)
            {
                client.Send(packet);
            }
        }
        public void BroadcastExceptSelf(int clientId, IMessage packet)
        {
            foreach (ClientSession p in _clients.Values)
            {
                if (p.SessionID != clientId)
                    p.Send(packet);
            }
        }
        #endregion;
    }
}
