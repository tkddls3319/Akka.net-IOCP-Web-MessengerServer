using Akka.Actor;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class RoomManagerActor : ReceiveActor
    {
        #region Message
        public class SetSessionManager
        {
            public IActorRef SessionManager { get; }
            public SetSessionManager(IActorRef sessionManager) => SessionManager = sessionManager;
        }
        public class AddRoom
        {
            public AddRoom() { }
        }
        public class RemoveRoom
        {
            public int RoomId { get; set; }
            public RemoveRoom(int roomId)
            {
                RoomId = roomId;
            }
        }
        public class AddClient
        {
            public ClientSession Session { get; }
            public AddClient(ClientSession session)
            {
                Session = session;
            }
        }
        #endregion

        #region Actor
        IActorRef _sessionManager;
        #endregion

        Dictionary<int, IActorRef> _rooms = new Dictionary<int, IActorRef>();
        int _roomCount = 0;

        public RoomManagerActor()
        {
            Receive<SetSessionManager>(msg => _sessionManager = msg.SessionManager);

            Receive<AddRoom>(msg => AddRoomHandler());
            Receive<RemoveRoom>(msg => RemoveRoomHandler(msg.RoomId));
            Receive<AddClient>(msg => AddClientToRoomHandler(msg.Session));
        }
        private void AddClientToRoomHandler(ClientSession session)
        {
            var roomResults = _rooms.Values.Select(room =>
            {
                var clientCount = room.Ask<int>(new RoomActor.GetClientCount()).Result;
                return new { Room = room, ClientCount = clientCount };
            }).ToArray();

            // 클라이언트 수가 5 이하인 첫 번째 룸 찾기
            var selectedRoom = roomResults.FirstOrDefault(r => r.ClientCount < 5)?.Room;

            if (selectedRoom == null)
            {
                AddRoomHandler(); // 새로운 룸 생성
                _rooms[_roomCount].Tell(new RoomActor.EnterClient(session)); // 새로 생성한 룸에 클라이언트 추가
            }
            else
            {
                selectedRoom.Tell(new RoomActor.EnterClient(session));
            }
        }
        void AddRoomHandler()
        {
            _roomCount++;
            var roomActor = Context.ActorOf(Props.Create(() => new RoomActor(Self, _sessionManager, _roomCount)), $"{_roomCount}");
            _rooms[_roomCount] = roomActor;
            Console.WriteLine($"Room {_roomCount} created.");
        }
        private void RemoveRoomHandler(int roomid)
        {
            if (_rooms.TryGetValue(roomid, out var roomActor))
            {
                Context.Stop(roomActor);
                _rooms.Remove(roomid);
                Console.WriteLine($"Room {roomid} removed. ");
            }
            else
            {
                Console.WriteLine($"Room {roomid} does not exist.");
            }
        }
    }
}
