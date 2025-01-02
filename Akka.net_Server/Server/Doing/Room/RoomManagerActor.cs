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
        public class AddRoom
        {
            public AddRoom() { }
        }
        public class RemoveRoom
        {
            public int RoomId { get; }
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

        Dictionary<int, IActorRef> _rooms = new Dictionary<int, IActorRef>();
        int _roomCount = 0;

        public RoomManagerActor()
        {
            Receive<AddRoom>(msg => AddHandler());
            Receive<RemoveRoom>(msg => RemoveHandler(msg.RoomId));
            Receive<AddClient>(msg => AddClientToRoomHandler(msg.Session));
        }

        private async Task AddClientToRoomHandler(ClientSession session)
        {
            //var sender = Sender;
            var roomTasks = _rooms.Values.Select(async room =>
            {
                int clientCount = await room.Ask<int>(new RoomActor.GetClientCount());
                return new { Room = room, ClientCount = clientCount };
            }).ToArray();

            try
            {
                var roomResults = await Task.WhenAll(roomTasks);

                // 클라이언트 수가 5 이하인 첫 번째 룸 찾기
                var selectedRoom = roomResults.FirstOrDefault(r => r.ClientCount <= 5)?.Room;

                if (selectedRoom == null)
                {
                    Console.WriteLine("No room with less than 5 clients found. Creating a new room...");
                    AddHandler(); // 새로운 룸 생성
                    _rooms[_roomCount].Tell(new RoomActor.EnterClient(session)); // 새로 생성한 룸에 클라이언트 추가
                }
                else
                {
                    selectedRoom.Tell(new RoomActor.EnterClient(session));
                    Console.WriteLine($"Client added to room with less than 5 clients.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to retrieve client counts: {ex.Message}");
            }
        }
        void AddHandler()
        {
            _roomCount++;
            var roomActor = Context.ActorOf(Props.Create(() => new RoomActor(_roomCount)), $"{_roomCount}");
            _rooms[_roomCount] = roomActor;
            Console.WriteLine($"Room {_roomCount} created.");
        }
        private void RemoveHandler(int roomid)
        {
            if (_rooms.TryGetValue(roomid, out var roomActor))
            {
                Context.Stop(roomActor);
                _rooms.Remove(roomid);
                Console.WriteLine($"Room {roomid} removed.");
            }
            else
            {
                Console.WriteLine($"Room {roomid} does not exist.");
            }
        }
    }
}
