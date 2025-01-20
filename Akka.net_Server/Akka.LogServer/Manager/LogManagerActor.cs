using Akka.Actor;
using Akka.Routing;

using Google.Protobuf.ClusterProtocol;
using Google.Protobuf.Protocol;

using Serilog;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//RoundRobinPool로 라우터 생성
//RoundRobinPool: 메시지를 라우티들에게 순서대로 전달.
//BroadcastPool: 모든 라우티에게 동일한 메시지를 전달.
//RandomPool: 라우티 중 무작위로 선택하여 메시지를 전달.
//ConsistentHashingPool: 메시지의 해시 값을 기반으로 라우티를 선택.
//TailChoppingPool: 메시지를 여러 라우티에게 차례로 보냄.
//ScatterGatherFirstCompletedPool: 메시지를 모든 라우티에 보내고 첫 번째 응답을 반환.

namespace Akka.LogServer
{
    public class LogManagerActor : ReceiveActor
    {
        public LogManagerActor()
        {
            #region Router
            IActorRef _chatWriteLogRouter;
            IActorRef _chatReadLogRouter;

            _chatWriteLogRouter = Context.ActorOf(Props.Create(() => new ChatLogWriteActor())
                .WithRouter(new RoundRobinPool(5)), "ChatLogWriteActor");

            _chatReadLogRouter = Context.ActorOf(Props.Create(() => new ChatLogReaderActor())
                .WithRouter(new RoundRobinPool(5)), "ChatLogReaderActor");
            #endregion

            Receive<SL_ChatWriteLog>(message =>
            {
                _chatWriteLogRouter.Tell(new WriteMessage<SL_ChatWriteLog>(Sender, message));
            });

            Receive<SL_ChatReadLog>(message =>
            {
                _chatReadLogRouter.Tell(new WriteMessage<SL_ChatReadLog>(Sender, message));
            });
        }
    }
}
