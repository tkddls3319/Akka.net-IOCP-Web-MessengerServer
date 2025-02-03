using Akka.Actor;
using Akka.Cluster;

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
                Member clusterMember = msg.Member;

                var actorAddr = Address.Parse(clusterMember.Address.ToString());

                //TODO : Roloes에 따라 달라지는데 어떻게 해야할지 좀 더 고민해봐야겠음.
                string clusterName = clusterMember.Roles.OrderBy(r=>r).ToList().FirstOrDefault();

                if (Enum.TryParse(clusterName, out ClusterType clusterType))
                    ClusterManager.Instance.InitClusterActor(actorAddr, clusterType);

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
                string clusterName = clusterMember.Roles.OrderBy(r => r).ToList().FirstOrDefault();

                if (Enum.TryParse(clusterName, out ClusterType clusterType))
                    ClusterManager.Instance.RemoveClusterActor(clusterType);

                Log.Logger.Information($"[ClusterListener] Cluster Node Removed : {msg.Member}");
                Log.Logger.Information($"[ClusterListener] Current Cluster Members: {string.Join(", ", cluster.State.Members)}");
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
