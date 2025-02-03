using Akka.Actor;

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
    public class ClusterManager
    {
        #region Singleton
        static readonly object _lock = new();

        static ClusterManager _instance;
        public static ClusterManager Instance { get { Init(); return _instance; } }
        #endregion

        #region Cluster
        readonly ActorSystem _actorSystem;

        private Dictionary<ClusterType, IActorRef> _clusterActors = new();
        #endregion

        static void Init()
        {
            if (_instance == null)
                lock (_lock)
                    _instance = new ClusterManager(Program.ServerActorSystem);
        }
        public ClusterManager(ActorSystem actorSystem)
        {
            _actorSystem = actorSystem;
        }

        #region LogCluster
        public void InitClusterActor(Address addr, ClusterType actorName)
        {
            SetClusterActor(addr, actorName);
        }
        void SetClusterActor(Address addr, ClusterType actorName)
        {
            if (_clusterActors.ContainsKey(actorName) == false)
            {
                Task.Run(async () =>
                {
                    try
                    {
                        var actorSelector = _actorSystem.ActorSelection($"{addr}/user/{actorName}");
                        var actorRef = await actorSelector.ResolveOne(TimeSpan.FromSeconds(5));//비동기적방식
                        //_clusterLogServer = actorSelector.ResolveOne(TimeSpan.FromSeconds(5)).Result;//동기적방식
                        //_clusterLogServer = actorSelector.ResolveOne(TimeSpan.FromSeconds(5)).GetAwaiter().GetResult();//예외처리적으로 안전한 동기적방식

                        lock (_lock)
                        {
                            if (_clusterActors.TryAdd(actorName, actorRef))
                                Log.Logger.Information($"[ClusterActorManager] Cluster actor '{actorName}' initialized successfully.");
                            else
                                Log.Logger.Error($"[ClusterActorManager] Cluster actor '{actorName}' initialized Unsuccessfully.");
                        }
                    }
                    catch (ActorNotFoundException err)
                    {
                        Log.Logger.Error($"[ClusterActorManager] Actor not found: {err.Message}");
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error($"[ClusterActorManager] Error initializing cluster actor '{actorName}': {ex.Message}");
                    }
                });
            }
        }
        public IActorRef GetClusterActor(ClusterType key)
        {
            lock (_lock)
            {
                return _clusterActors.TryGetValue(key, out var actorRef) ? actorRef : null;
            }
        }
        public bool RemoveClusterActor(ClusterType key)
        {
            lock (_lock)
            {
                return _clusterActors.Remove(key, out var actorRef);
            }
        }
        #endregion
    }
}
