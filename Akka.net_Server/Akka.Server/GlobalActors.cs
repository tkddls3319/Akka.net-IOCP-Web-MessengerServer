using Akka.Actor;
using Akka.ClusterCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Akka.Server
{
    public static class GlobalActors
    {
        private static readonly Lazy<IActorRef> _clusterManager = new(() =>
            Program.ServerActorSystem.ActorOf(ClusterManagerActor.Props(), "clusterActor-manager"));

        public static IActorRef ClusterManager => _clusterManager.Value;
    }
}
