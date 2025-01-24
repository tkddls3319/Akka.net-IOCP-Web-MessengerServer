using Akka.AccountServer.DB;
using Akka.Actor;
using Akka.Streams.Stage;

using Google.Protobuf.ClusterProtocol;

using Microsoft.EntityFrameworkCore;

using static Akka.AccountServer.Actor.AccountActor;

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
                AppDbContext context = message.Context;
                LoginAccountPacketReq req = message.Message;

                LoginAccountPacketRes res = new LoginAccountPacketRes();

                AccountDb account = context.Accounts
                                        .AsNoTracking()
                                        .Where(a => a.AccountName == req.AccountName && a.Password == req.Password)
                                        .FirstOrDefault();

                if (account == null)
                {
                    res.LoginOk = false;
                }
                else
                {
                    res.LoginOk = true;

                    //TODO : Clusetr Server에서 룸받아오기
                    {
                        var seedNodes = Akka.Cluster.Cluster.Get(_actorSystem).Settings.SeedNodes[0];
                        //TODO : Cluster를 관리하는 고용적 라이브러리 메니저 필요할 것 같음 Akka.Server에 만들어 놨으나 이걸 튜닝해서 라이브러리화 해야할 것 같음.
                        SA_GetAllRoomInfo response = await _actorSystem.ActorSelection($"{seedNodes}/user/RoomManagerActor").Ask<SA_GetAllRoomInfo>(new AS_GetAllRoomInfo(), TimeSpan.FromSeconds(5));
                     
                        res.RoomList = new List<RoomInfo>();
                        foreach (var room in response.RoomId)
                        {
                            res.RoomList.Add(new RoomInfo() { RoomId = room.ToString() });
                        }
                    }
                }

                Sender.Tell(res);
            });
        }
    }
}
