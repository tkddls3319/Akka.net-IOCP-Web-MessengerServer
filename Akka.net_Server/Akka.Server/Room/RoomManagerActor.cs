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
        class Room
        {
            public IActorRef Actor { get; set; }
            public int UserCount { get; set; }

            public Room(IActorRef actor, int userCount)
            {
                Actor = actor;
                UserCount = userCount;
            }
        }

        #region Message
        public record CreateRoomCommand ();
        public record RemoveRoomCommand(int RoomId);
        public record AddClientCommand(ClientSession Session, int RoomId);
        public record CreateRoomAndAddClientCommand(ClientSession Session);
        public record MultiTestRoomCommand(ClientSession Session);
        public record ChangeUserCountCommand(int RoomId, int UserCount);
        #endregion

        #region Actor
        IActorRef _sessionManager;
        #endregion

        Dictionary<int, Room> _rooms = new Dictionary<int, Room>();
        int _roomCount = 0;

        public RoomManagerActor(IActorRef sessionManager)
        {
            _sessionManager = sessionManager;

            Receive<CreateRoomCommand>(msg => CreateRoomHandler());
            Receive<RemoveRoomCommand>(msg => RemoveRoomHandler(msg.RoomId));
            Receive<AddClientCommand>(msg => AddClientHandler(msg));
            Receive<CreateRoomAndAddClientCommand>(msg => CreateRoomAndAddClientHandler(msg));
            Receive<MultiTestRoomCommand>(msg => MultiTestRoomHandler(msg));
            Receive<ChangeUserCountCommand>(msg => ChangeUserCountHandler(msg));

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

                    var clientCount = roomInfo.Value.Actor.Ask<int>(new RoomActor.GetClientCountQuery()).Result;
                    room.CurrentCount = clientCount;

                    packet.RoomInfos.Add(room);
                }

                Sender.Tell(packet);
            });
            #endregion
        }

        private void ChangeUserCountHandler(ChangeUserCountCommand msg)
        {
            if (_rooms.TryGetValue(msg.RoomId, out var roomInfo))
                roomInfo.UserCount = msg.UserCount;
        }

        protected override void PreStart()
        {
            base.PreStart();

            for (int i = 0; i < 5; i++)
            {
                CreateRoomHandler();
            }
        }
        IActorRef CreateRoomHandler()
        {
            _roomCount++;
            var roomActor = Context.ActorOf(Props.Create(() => new RoomActor(_sessionManager, _roomCount)), $"{_roomCount}");
            _rooms[_roomCount] = new Room(roomActor, 0);

            Log.Logger.Information($"[RoomManager] Room{_roomCount} Created. Room Count : {_rooms.Count}");
            return roomActor;
        }
        void AddClientHandler(AddClientCommand session)
        {
            int roomId = session.RoomId;

            if (_rooms[roomId].UserCount < Define.RoomMaxCount)
            {
                _rooms[roomId].Actor.Tell(new RoomActor.EnterClientCommand(session.Session));
            }
            else
            {
                // TODO: 방이 꽉 차서 다시 선택해야 하는 로직 추가 필요
            }

            //비동기 방식으로 해당 버전 사용해도 상관없음.
            //_rooms[roomId].actor.Ask<int>(new RoomActor.GetClientCountQuery(), TimeSpan.FromSeconds(3))
            //     .PipeTo(Self, Sender, success: count =>
            //     {
            //         if (count < Define.RoomMaxCount)
            //         {
            //             _rooms[roomId].actor.Tell(new RoomActor.EnterClientCommand(session.Session));
            //         }
            //         else
            //         {
            //             // TODO: 방이 꽉 차서 다시 선택해야 하는 로직 추가 필요
            //         }
            //         return null;
            //     });
        }
        private void CreateRoomAndAddClientHandler(CreateRoomAndAddClientCommand session)
        {
            CreateRoomHandler();
            Self.Tell(new AddClientCommand(session.Session, _roomCount));//Deadlock 방지
        }

        private void RemoveRoomHandler(int roomid)
        {
            if (_rooms.TryGetValue(roomid, out var roomActor))
            {
                Context.Stop(roomActor.Actor);
                _rooms.Remove(roomid);
                Log.Logger.Information($"[RoomManager] Room{roomid} Remove. Room Count : {_rooms.Count}");
            }
            else
            {
                Log.Logger.Information($"[RoomManager] Room{roomid} does not exist.");
            }
        }
        private async Task MultiTestRoomHandler(MultiTestRoomCommand msg)
        {
            var selectedRoom = _rooms.Values.FirstOrDefault(r => r.UserCount < Define.RoomMaxCount);

            if (selectedRoom == null)
            {
                // 새로운 룸 생성, 새로 생성한 룸에 클라이언트 추가
                var newRoom = CreateRoomHandler();
                newRoom.Tell(new RoomActor.EnterClientCommand(msg.Session));
            }
            else
            {
                selectedRoom.Actor?.Tell(new RoomActor.EnterClientCommand(msg.Session));
            }

            //비동기 방식으로 해당 버전 사용해도 상관없음.
            //var roomResults = _rooms.Values.Select(r =>
            //{
            //    var clientCount = r.Actor.Ask<int>(new RoomActor.GetClientCountQuery()).Result;
            //    return new { Room = r.Actor, ClientCount = clientCount };
            //}).ToArray();

            //var selectedRoom = roomResults.FirstOrDefault(r => r.ClientCount < Define.RoomMaxCount)?.Room;

            //if (selectedRoom == null)
            //{
            //    // 새로운 룸 생성, 새로 생성한 룸에 클라이언트 추가
            //    CreateRoomHandler().Tell(new RoomActor.EnterClientCommand(msg.Session));
            //}
            //else
            //{
            //    selectedRoom.Tell(new RoomActor.EnterClientCommand(msg.Session));
            //}
        }
    }
}
