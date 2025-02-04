using Akka.Actor;

using Google.Protobuf.ClusterProtocol;

using Serilog;

using ServerCore;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Akka.Server
{
    public class RoomManagerActor : ReceiveActor
    {
        #region Message
        public record SetSessionManagerCommand(IActorRef SessionManager);
        public record CreateRoomCommand ();
        public record RemoveRoomCommand(int RoomId);
        public record AddClientCommand(ClientSession Session, int RoomId);
        public record CreateRoomAndAddClientCommand(ClientSession Session);
        #endregion

        #region Actor
        IActorRef _sessionManager;
        #endregion

        Dictionary<int, IActorRef> _rooms = new Dictionary<int, IActorRef>();
        int _roomCount = 0;


        public RoomManagerActor(IActorRef sessionManager)
        {
            _sessionManager = sessionManager;

            Receive<SetSessionManagerCommand>(msg => _sessionManager = msg.SessionManager);
            Receive<CreateRoomCommand>(msg => CreateRoomHandler());
            Receive<RemoveRoomCommand>(msg => RemoveRoomHandler(msg.RoomId));
            Receive<AddClientCommand>(msg => AddClientHandler(msg));
            Receive<CreateRoomAndAddClientCommand>(msg => CreateRoomAndAddClientHandler(msg));

            #region Cluster
            Receive<AS_GetAllRoomInfoQuery>(msg =>
            {
                SA_GetAllRoomInfoResponse packet = new SA_GetAllRoomInfoResponse();

                foreach (var roomInfo in _rooms)
                {
                    var room = new RoomInfo()
                    {
                        RoomID = roomInfo.Key,
                        MaxCount = Define.RoomMaxCount,
                    };

                    var clientCount = roomInfo.Value.Ask<int>(new RoomActor.GetClientCountQuery()).Result;
                    room.CurrentCount = clientCount;

                    packet.RoomInfos.Add(room);
                }

                Sender.Tell(packet);
            });
            #endregion
        }

        protected override void PreStart()
        {
            base.PreStart();

            for (int i = 0; i < 5; i++)
            {
                CreateRoomHandler();
            }
        }
        void CreateRoomHandler()
        {
            _roomCount++;
            var roomActor = Context.ActorOf(Props.Create(() => new RoomActor(Self, _sessionManager, _roomCount)), $"{_roomCount}");
            _rooms[_roomCount] = roomActor;
            Log.Logger.Information($"[RoomManager] Room{_roomCount} Created. Room Count : {_rooms.Count}");
        }
        void AddClientHandler(AddClientCommand session)
        {
            int roomId = session.RoomId;

            _rooms[roomId].Ask<int>(new RoomActor.GetClientCountQuery(), TimeSpan.FromSeconds(3))
                     .PipeTo(Self, Sender, success: count =>
                     {
                         if (count < Define.RoomMaxCount)
                         {
                             _rooms[roomId].Tell(new RoomActor.EnterClientCommand(session.Session));
                         }
                         else
                         {
                             // TODO: 방이 꽉 차서 다시 선택해야 하는 로직 추가 필요
                         }
                         return null;
                     });
        }
        private void CreateRoomAndAddClientHandler(CreateRoomAndAddClientCommand session)
        {
            CreateRoomHandler();

            Self.Tell(new AddClientCommand(session.Session, _roomCount));//Deadlock 방지

            //TODO : 랜덤 방입장 기능 만들 때 사용 예정.
            //var roomResults = _rooms.Values.Select(room =>
            //{
            //    var clientCount = room.Ask<int>(new RoomActor.GetClientCountMessage()).Result;
            //    return new { Room = room, ClientCount = clientCount };
            //}).ToArray();

            //// 클라이언트 수가 5 이하인 첫 번째 룸 찾기
            //var selectedRoom = roomResults.FirstOrDefault(r => r.ClientCount < Define.RoomMaxCount)?.Room;

            //if (selectedRoom == null)
            //{
            //    AddRoomHandler(); // 새로운 룸 생성
            //    _rooms[_roomCount].Tell(new RoomActor.EnterClientMessage(session)); // 새로 생성한 룸에 클라이언트 추가
            //}
            //else
            //{
            //    selectedRoom.Tell(new RoomActor.EnterClientMessage(session));
            //}
        }

        private void RemoveRoomHandler(int roomid)
        {
            if (_rooms.TryGetValue(roomid, out var roomActor))
            {
                Context.Stop(roomActor);
                _rooms.Remove(roomid);
                Log.Logger.Information($"[RoomManager] Room{roomid} Remove. Room Count : {_rooms.Count}");
            }
            else
            {
                Log.Logger.Information($"[RoomManager] Room{roomid} does not exist.");
            }
        }
    }
}
