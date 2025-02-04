using Akka.Actor;
using Akka.Cluster;
using Akka.ClusterCore;

using Google.Protobuf.Protocol;

using Serilog;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Akka.Server.Define;

namespace Akka.Server
{
    public class ClusterListenerActor : ReceiveActor
    {
        private readonly Cluster.Cluster _cluster = Cluster.Cluster.Get(Context.System);

        public ClusterListenerActor()
        {
            // 클러스터에 새로운 노드가 참가했을 때 발생
            Receive<ClusterEvent.MemberJoined>(msg =>
            {
                var clusterMember = msg.Member;
                var actorAddr = clusterMember.Address;

                // 역할 중 알파벳순으로 가장 앞에 있는 것 선택
                string clusterName = clusterMember.Roles.Min();

                //새로들어온 클러스터에 맞게 모든액터를 세팅
                if (Enum.TryParse(clusterName, out ClusterType clusterType))
                {
                    switch (clusterType)
                    {
                        case ClusterType.LogServer:
                            InitializeActors<LogServerActorType>(actorAddr);
                            break;

                        case ClusterType.AccountServer:
                            InitializeActors<AccountServerActorType>(actorAddr);
                            break;
                    }
                }

                Log.Logger.Information($"[ClusterListener] Cluster Node Joined: {msg.Member}");
            });
            //정상적으로 활성화
            Receive<ClusterEvent.MemberUp>(msg =>
            {
                Log.Logger.Information($"[ClusterListener] Cluster Node is Up : {msg.Member}");
            });

            Receive<ClusterEvent.MemberRemoved>(msg =>
            {
                var cluster = Cluster.Cluster.Get(Context.System);

                Member clusterMember = msg.Member;
                var actorAddr = clusterMember.Address;

                // 역할 중 알파벳순으로 가장 앞에 있는 것 선택
                string clusterName = clusterMember.Roles.Min();

                //새로들어온 클러스터에 맞게 모든액터를 세팅
                if (Enum.TryParse(clusterName, out ClusterType clusterType))
                {
                    switch (clusterType)
                    {
                        case ClusterType.LogServer:
                            RemoveActors<LogServerActorType>();
                            break;

                        case ClusterType.AccountServer:
                            RemoveActors<AccountServerActorType>();
                            break;
                    }
                }

                Log.Logger.Information($"[ClusterListener] Cluster Node Removed : {msg.Member}");
                Log.Logger.Information($"[ClusterListener] Current Cluster Members: {string.Join(", ", cluster.State.Members)}");
            });

            Receive<ClusterEvent.LeaderChanged>(msg =>
            {
                Log.Logger.Information($"[ClusterListener] Cluster Leader Changed: : {msg.Leader}");
            });
        }

        private void InitializeActors<T>(Address actorAddr) where T : Enum
        {
            foreach (var actorType in Enum.GetValues(typeof(T)).Cast<T>())
            {
                GlobalActors.ClusterManager.Tell(new InitClusterActorCommand(actorAddr, actorType));
            }
        }
       void RemoveActors<T>() where T : Enum
        {
            foreach (var actorType in Enum.GetValues(typeof(T)).Cast<T>())
            {
                GlobalActors.ClusterManager.Tell(new RemoveClusterActorQuery(actorType));
            }
        }

        protected override void PreStart()
        {
            // 클러스터 이벤트 구독
            _cluster.Subscribe(Self,
                ClusterEvent.SubscriptionInitialStateMode.InitialStateAsEvents,
                typeof(ClusterEvent.IMemberEvent), // Member 이벤트
                typeof(ClusterEvent.LeaderChanged) // 리더 변경 이벤트
            );
        }
        protected override void PostStop()
        {
            // 클러스터 이벤트 구독 해제
            _cluster.Unsubscribe(Self);
        }
    }
}
