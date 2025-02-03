using Akka.AccountServer.DB;
using Akka.AccountServer.Define;
using Akka.Actor;
using Akka.ClusterCore;
using Akka.Streams.Stage;

using Google.Protobuf.ClusterProtocol;

using Microsoft.EntityFrameworkCore;

namespace Akka.AccountServer.Actor
{
    public class AccountActor : ReceiveActor
    {
        #region Message
        public class AccountMessage<T>
        {
            public AppDbContext Context { get; }
            public T Message { get; }

            public AccountMessage(AppDbContext context, T message)
            {
                Context = context;
                this.Message = message;
            }
        }
        #endregion

        ActorSystem _actorSystem;
        public AccountActor(ActorSystem actorSystem)
        {
            _actorSystem = actorSystem;

            //회원가입
            Receive<AccountMessage<CreateAccountPacketReq>>((message) =>
            {
                AppDbContext context = message.Context;
                CreateAccountPacketReq req = message.Message;

                CreateAccountPacketRes res = new CreateAccountPacketRes();

                AccountDb account = context.Accounts
                                          .AsNoTracking()//read only로 성능 향상
                                          .Where(a => a.AccountName == req.AccountName)
                                          .FirstOrDefault();

                if (account == null)
                {
                    context.Accounts.Add(new AccountDb()
                    {
                        AccountName = req.AccountName,
                        Password = req.Password,
                    });

                    bool success = context.SaveChangesEx();
                    res.CreateOk = success;
                }
                else
                {
                    res.CreateOk = false;
                }

                Sender.Tell(res);
            });

            //로그인
            Receive<AccountMessage<LoginAccountPacketReq>>(async (message) =>
            {
                var task = HandleLogin(message);
                task.PipeTo(Sender, Self);
            });
        }
        private async Task<LoginAccountPacketRes> HandleLogin(AccountMessage<LoginAccountPacketReq> message)
        {
            var context = message.Context;
            var req = message.Message;

            var res = new LoginAccountPacketRes();
            var account = await context.Accounts
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(a => a.AccountName == req.AccountName && a.Password == req.Password);

            if (account == null)
            {
                res.LoginOk = false;
            }
            else
            {
                res.LoginOk = true;

                //Akka.Server에 모든 방 정보 받아오는 부분.
                {

                    var clusterLocator = new ClusterActorLocator(_actorSystem);

                    var response = await clusterLocator.AskClusterActor<SA_GetAllRoomInfo>(
                        $"{TcpServerActorType.RoomManagerActor}",
                        new AS_GetAllRoomInfo(),
                        TimeSpan.FromSeconds(5)
                    );

                    res.RoomList = response.RoomId.Select(room => new RoomInfo { RoomId = room.ToString() }).ToList();

                    //var seedNodes = Akka.Cluster.Cluster.Get(_actorSystem).Settings.SeedNodes[0];
                    //var response = await _actorSystem.ActorSelection($"{seedNodes}/user/{ClusterActorType.RoomManagerActor}")
                    //                                 .Ask<SA_GetAllRoomInfo>(new AS_GetAllRoomInfo(), TimeSpan.FromSeconds(5));

                    //res.RoomList = response.RoomId.Select(room => new RoomInfo { RoomId = room.ToString() }).ToList();
                }
            }

            return res;
        }
    }
}
