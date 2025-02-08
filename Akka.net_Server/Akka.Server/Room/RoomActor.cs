using Akka.Actor;
using Akka.ClusterCore;

using Google.Protobuf;
using Google.Protobuf.ClusterProtocol;
using Google.Protobuf.Protocol;
using Google.Protobuf.WellKnownTypes;

using Serilog;

using ServerCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;alksdjflasjldk;fj;alkshdfoahoudfhoahohuoh;Half;ljdlsafjl;saj
using System.Threading.Tasks;

namespace Akka.Server
{
    public class RoomActor : ReceiveActor
    {
        #region Message
        public record GetClientCountQuery();
        public record EnterClientCommand(ClientSession Session);
        public record LeaveClientCommand(int ClientId, bool Disconnected);

        #endregion

        #region Actor
        IActorRef _sessionManager;
        #endregion
        public int RoomID { get; set; }
        Dictionary<int, ClientSession> _clients = new Dictionary<int, ClientSession>();
        int _clinetCount { get { return _clients.Count; } }

        public RoomActor(IActorRef sessionManagerActor, int roomNumber)
        {
            _sessionManager = sessionManagerActor;

            RoomID = roomNumber;

            Receive<GetClientCountQuery>(_ => Sender.Tell(_clients.Count));
            Receive<EnterClientCommand>(msg => EnterClientHandler(msg));
            Receive<LeaveClientCommand>(msg => LeaveClientHandler(msg.ClientId, msg.Disconnected));
            Receive<MessageCustomCommand<ClientSession, C_Chat>>(msg => ChatHandle(msg.Item1, msg.Item2));
        }
        private void EnterClientHandler(EnterClientCommand client)
        {
            if (client.Session == null)
                return;

            if(_clinetCount >= Define.RoomMaxCount)
            {
                Context.Parent.Tell(new RoomManagerActor.MultiTestRoomCommand(client.Session));
                return;
            }

            ClientSession session = client.Session;
            if (_clients.ContainsKey(session.SessionID))
                return;

            _clients[session.SessionID] = session;
            session.Room = Self;

            {
                Context.Parent.Tell(new RoomManagerActor.ChangeUserCountCommand(RoomID, _clinetCount));
            }

            {
                // 신규 유저에게 서버 입장 정보 전송
                client.Session.Send(new S_EnterServer
                {
                    Client = new ClientInfo
                    {
                        ObjectId = client.Session.SessionID,
                        RoomID = RoomID,
                        ClientCount = _clinetCount
                    }
                });
            }

            //채팅룸안에 모든 사용자 전달
            {
                var spawnPacket = new S_Spawn { ClientCount = _clinetCount };
                foreach (var p in _clients.Values)
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
                var notifyPacket = new S_Spawn
                {
                    ClientCount = _clinetCount,
                    ObjectIds = { client.Session.SessionID },
                    AccountNames = { client.Session.AccountName }
                };
                BroadcastExceptSelf(client.Session.SessionID, notifyPacket);
            }

            //이전 모든 채팅을 읽어 새로온 사용자에게 전달
            {
                if (client.Session.AccountName != "AI")//ai가 아니라면
                {
                    GlobalActors.ClusterManager.Ask<IActorRef>(
                            new GetClusterActorQuery(Define.LogServerActorType.LogManagerActor),
                            TimeSpan.FromSeconds(3)
                        ).ContinueWith(task =>
                        {
                            if (task.Status != TaskStatus.RanToCompletion || task.Result == ActorRefs.Nobody)
                            {
                                Console.WriteLine("해당 액터를 찾을 수 없음.");
                                return;
                            }

                            var actorRef = task.Result;

                            actorRef.Ask<LS_ChatReadLogResponse>(
                                new SL_ChatReadLogQuery() { RoomId = this.RoomID },
                                TimeSpan.FromSeconds(5)
                            ).ContinueWith(responseTask =>
                            {
                                if (responseTask.Status == TaskStatus.RanToCompletion && responseTask.Result?.Chats != null)
                                {
                                    foreach (var chat in responseTask.Result.Chats)
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
                                else
                                {
                                    Log.Logger.Warning("채팅 데이터를 가져오지 못했습니다.");
                                }
                            }, TaskContinuationOptions.ExecuteSynchronously);
                        }, TaskContinuationOptions.ExecuteSynchronously);
                }

                //동기적인 방법
                //LS_ChatReadLog response = ClusterManager.Instance.GetClusterActor(Define.LogServerActorType.LogManagerActor)
                //    ?.Ask<LS_ChatReadLog>(new SL_ChatReadLog()
                //    {
                //        RoomId = this.RoomID
                //    }, TimeSpan.FromSeconds(3)).Result;

                //if (response?.Chats != null)
                //{
                //    foreach (var chat in response.Chats)
                //    {
                //        S_Chat readChat = new S_Chat()
                //        {
                //            Chat = chat.Chat,
                //            AccountName = chat.AccoutnName,
                //            ObjectId = chat.ObjectId,
                //            Time = chat.Time,
                //        };

                //        client.Session.Send(readChat);
                //    }
                //}
            }

            Log.Logger.Information($"[Room{RoomID}] Enter Client ID : {session.SessionID}");
        }

        private void LeaveClientHandler(int clientId, bool disconnected)
        {
            ClientSession client = null;
            if (_clients.Remove(clientId, out client) == false)
                return;

            client.Room = null;

            {
                Context.Parent.Tell(new RoomManagerActor.ChangeUserCountCommand(RoomID, _clinetCount));
            }

            //룸정보 전달
            {
                S_LeaveServer leavPacket = new S_LeaveServer();
                client.Send(leavPacket);
            }

            //타인에게 전달
            {
                S_Despawn despawnPacket = new S_Despawn();
                despawnPacket.ObjectId = clientId;
                despawnPacket.ClientCount = _clinetCount;
                despawnPacket.AccountName = client.AccountName;
                BroadcastExceptSelf(clientId, despawnPacket);
            }

            //Room안에 사용자 0명이면 제거
            if (_clients.Count == 0)
                Context.Parent.Tell(new RoomManagerActor.RemoveRoomCommand(RoomID));

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
                Time = chatPacket.Time,
            };

            //LogServer클러스터에 Log 전달
            {
                SL_ChatWriteLogCommand logPacket = new()
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

                GlobalActors.ClusterManager.Tell(new SendClusterActorCommand(Define.LogServerActorType.LogManagerActor, logPacket));
            }

            BroadcastExceptSelf(id, severChatPacket);
        }

        #region Util
        public void BroadCast(IMessage packet)
        {
            var sendLists = _clients.Values.ToList();
            Parallel.ForEach(sendLists, client =>
            {
                client.Send(packet);
            });
        }

        public void BroadcastExceptSelf(int clientId, IMessage packet)
        {
            var sendLists = _clients.Values.Where(p => p.SessionID != clientId).ToList();
            Parallel.ForEach(sendLists, client =>
            {
                client.Send(packet);
            });
        }
        //public void BroadCast(IMessage packet)
        //{
        //    foreach (ClientSession client in _clients.Values)
        //    {
        //        client.Send(packet);
        //    }
        //}
        //public void BroadcastExceptSelf(int clientId, IMessage packet)
        //{
        //    var sendLists = _clients.Values.ToList();

        //    Task.Run(() =>
        //    {
        //        foreach (ClientSession p in sendLists)
        //        {
        //            if (p.SessionID != clientId)
        //                p.Send(packet);
        //        }
        //    });
        //}
        #endregion;
    }
}
