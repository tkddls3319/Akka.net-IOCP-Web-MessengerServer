using Akka.Actor;
using Akka.Cluster;
using Google.Protobuf.Protocol;

using Serilog;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Akka.Server
{
    public class ClusterListenerActor : ReceiveActor
    {
        private readonly Cluster.Cluster _cluster = Cluster.Cluster.Get(Context.System);

        public ClusterListenerActor()
        {
            // 클러스터 이벤트 수신
            Receive<ClusterEvent.MemberJoined>(msg =>
            {
                Log.Logger.Information($"[ClusterListener] Cluster Node Joined: {msg.Member}");
                if (msg.Member.Roles.Any(role => role.Contains("logAkka")))
                {
                    var actorAddr = Address.Parse(Define.AddrLogManagerActor);
                    ClusterActorManager.Instance.InitClusterActor(actorAddr, Define.ClusterType.LogManagerActor);
                }
            });

            Receive<ClusterEvent.MemberUp>(msg =>
            {
                Log.Logger.Information($"[ClusterListener] Cluster Node is Up : {msg.Member}");
            });

            Receive<ClusterEvent.MemberRemoved>(msg =>
            {
                var cluster = Cluster.Cluster.Get(Context.System);
                Log.Logger.Information($"[ClusterListener] Cluster Node Removed : {msg.Member}");
                Log.Logger.Information($"[ClusterListener] Current Cluster Members: {string.Join(", ", cluster.State.Members)}");

                if (msg.Member.Roles.Any(role => role.Contains("logAkka")))
                {
                    ClusterActorManager.Instance.RemoveClusterActor(Define.ClusterType.LogManagerActor);
                }
            });

            Receive<ClusterEvent.LeaderChanged>(msg =>
            {
                Log.Logger.Information($"[ClusterListener] Cluster Leader Changed: : {msg.Leader}");
            });
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
