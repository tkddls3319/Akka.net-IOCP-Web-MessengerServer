using Akka.Actor;

using Google.Protobuf.ClusterProtocol;

using Serilog;

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
        public class SetSessionManagerMessage
        {
            public IActorRef SessionManager { get; }
            public SetSessionManagerMessage(IActorRef sessionManager) => SessionManager = sessionManager;
        }
        public class AddRoomMessage
        {
            public AddRoomMessage() { }
        }
        public class RemoveRoomMessage
        {
            public int RoomId { get; set; }
            public RemoveRoomMessage(int roomId)
            {
                RoomId = roomId;
            }
        }
        public class AddClientMessage
        {
            public ClientSession Session { get; }
            public int RoomId { get; }
            public AddClientMessage(ClientSession session, int roomId)
            {
                Session = session;
                RoomId = roomId;
            }
        }
        #endregion

        #region Actor
        IActorRef _sessionManager;
        #endregion

        Dictionary<int, IActorRef> _rooms = new Dictionary<int, IActorRef>();
        int _roomCount = 0;

        public RoomManagerActor(IActorRef sessionManager)
        {
            _sessionManager = sessionManager;
            Receive<SetSessionManagerMessage>(msg => _sessionManager = msg.SessionManager);

            Receive<AddRoomMessage>(msg => AddRoomHandler());
            Receive<RemoveRoomMessage>(msg => RemoveRoomHandler(msg.RoomId));
            Receive<AddClientMessage>(msg => AddClientToRoomHandler(msg));

            #region Cluster
            Receive<AS_GetAllRoomInfo>(msg =>
            {
                SA_GetAllRoomInfo packet = new SA_GetAllRoomInfo();

                foreach (var roomInfo in _rooms)
                {
                    var room = new RoomInfo()
                    {
                        RoomID = roomInfo.Key,
                        MaxCount = Define.RoomMaxCount,
                    };

                    var clientCount = roomInfo.Value.Ask<int>(new RoomActor.GetClientCountMessage()).Result;
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
            AddRoomHandler();
        }
        private void AddClientToRoomHandler(AddClientMessage session)
        {
            int roomId = session.RoomId;
            var roomResults = _rooms[roomId].Ask<int>(new RoomActor.GetClientCountMessage()).Result;

            if(roomResults < Define.RoomMaxCount)
            {
                 _rooms[roomId].Tell(new RoomActor.EnterClientMessage(session.Session)); // 새로 생성한 룸에 클라이언트 추가
            }
            else
            {
                //TODO: 꽉차서 방 못 들어가서 다시선택 해야하는 패킷 만들어야함
            }

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
        void AddRoomHandler()
        {
            _roomCount++;
            var roomActor = Context.ActorOf(Props.Create(() => new RoomActor(Self, _sessionManager, _roomCount)), $"{_roomCount}");
            _rooms[_roomCount] = roomActor;
            Log.Logger.Information($"[RoomManager] Room{_roomCount} Created. Room Count : {_rooms.Count}");
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
