using Akka.Actor;
using Akka.Cluster;
using Google.Protobuf.Protocol;
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
                Console.WriteLine($"Node joined: {msg.Member}");

                if (msg.Member.Roles.Any(role => role.Contains("logAkka")))
                {
                    var actorAddr = Address.Parse(ClusterActorManager.AddrLogManagerActor);
                    ClusterActorManager.Instance.InitClusterActor(actorAddr, ClusterActorManager.DefineClusterName.LogManagerActor);
                }
            });

            Receive<ClusterEvent.MemberUp>(msg =>
            {
                Console.WriteLine($"Node is up: {msg.Member}");
            });

            Receive<ClusterEvent.MemberRemoved>(msg =>
            {
                Console.WriteLine($"Node removed: {msg.Member}");

                var cluster = Cluster.Cluster.Get(Context.System);
                Console.WriteLine($"Cluster Members: {string.Join(", ", cluster.State.Members)}");

                if (msg.Member.Roles.Any(role => role.Contains("logAkka")))
                {
                    ClusterActorManager.Instance.RemoveClusterActor(ClusterActorManager.DefineClusterName.LogManagerActor);
                }
            });

            Receive<ClusterEvent.LeaderChanged>(msg =>
            {
                Console.WriteLine($"Leader changed: {msg.Leader}");
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
