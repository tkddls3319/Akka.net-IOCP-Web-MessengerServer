using Akka.Actor;

using Google.Protobuf.Protocol;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Akka.Server
{
    public class ClusterActorManager
    {
        #region Define Cluster Info 
        public const string AddrLogManagerActor = "akka.tcp://ClusterSystem@localhost:5001";
        public enum DefineClusterName
        {
            LogManagerActor,
        }
        #endregion

        #region Singleton
        static readonly object _lock = new();

        static ClusterActorManager _instance;
        public static ClusterActorManager Instance { get { Init(); return _instance; } }
        #endregion

        #region Cluster
        readonly ActorSystem _actorSystem;
        private Dictionary<DefineClusterName, IActorRef> _clusterActors = new();
        #endregion

        static void Init()
        {
            if (_instance == null)
                lock (_lock)
                    _instance = new ClusterActorManager(Program.ServerActorSystem);
        }
        public ClusterActorManager(ActorSystem actorSystem)
        {
            _actorSystem = actorSystem;
        }

        #region LogCluster
        public void InitClusterActor(Address addr, DefineClusterName actorName)
        {
            if (Cluster.Cluster.Get(_actorSystem).State.Members.Any(m => m.Address.ToString() == $"{addr}"))
                SetClusterActor(addr, actorName);
            else
                RemoveClusterActor(actorName);
        }
        void SetClusterActor(Address addr, DefineClusterName actorName)
        {
            if (!_instance._clusterActors.ContainsKey(actorName))
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
                            if (_instance._clusterActors.TryAdd(actorName, actorRef) == false)
                                Console.WriteLine("Init Cluster Actor ERR");
                            else
                                Console.WriteLine($"Cluster actor '{actorName}' initialized successfully.");
                        }
                    }
                    catch (ActorNotFoundException err)
                    {
                        Console.WriteLine($"Actor not found: {err.Message}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error initializing cluster actor '{actorName}': {ex.Message}");
                    }
                });
            }
        }
        public IActorRef GetClusterActor(DefineClusterName key)
        {
            lock (_lock)
            {
                return _instance._clusterActors.TryGetValue(key, out var actorRef) ? actorRef : null;
            }
        }
        public bool RemoveClusterActor(DefineClusterName key)
        {
            lock (_lock)
            {
                return _instance._clusterActors.Remove(key, out var actorRef);
            }
        }
        #endregion
    }
}
