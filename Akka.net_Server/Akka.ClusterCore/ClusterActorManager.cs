using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

using Akka.Actor;
using Akka.Cluster;
using Akka.Event;

using Google.Protobuf;

namespace Akka.ClusterCore
{
    public class ClusterManagerActor : ReceiveActor
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();
        private readonly ConcurrentDictionary<Enum, IActorRef> _clusterActors = new();

        public ClusterManagerActor()
        {
            Receive<InitClusterActor>(msg => InitClusterActorHandle(msg));
            Receive<ClusterActorResolved>(msg => RegisterClusterActor(msg));
            Receive<GetClusterActor>(msg => Sender.Tell(GetClusterActorHandle(msg)));
            Receive<RemoveClusterActor>(msg => Sender.Tell(RemoveClusterActorHandle(msg)));
            Receive<SendClusterActor>(msg => SendClusterActorHandle(msg));
        }

        /// <summary>
        /// `ActorSelection`을 사용하여 액터를 찾고 `PipeTo(Self)`로 전달
        /// </summary>
        private void InitClusterActorHandle(InitClusterActor msg)
        {
            if (_clusterActors.ContainsKey(msg.ActorType))
                return;

            var actorSelector = Context.ActorSelection($"{msg.Address}/user/{msg.ActorType}");

            actorSelector.ResolveOne(TimeSpan.FromSeconds(5))
                .PipeTo(
                    Self,
                    success: actorRef => new ClusterActorResolved(msg.ActorType, actorRef),
                    failure: ex => new ClusterActorResolved(msg.ActorType, ActorRefs.Nobody) // 실패 시 처리
                );
        }

        /// <summary>
        /// `PipeTo(Self)`에서 받은 메시지를 처리하여 `_clusterActors`에 저장
        /// </summary>
        private void RegisterClusterActor(ClusterActorResolved msg)
        {
            if (msg.ActorRef == ActorRefs.Nobody)
            {
                _log.Warning($"[ClusterManagerActor] Failed to resolve actor: {msg.ActorType}");
                return;
            }

            if (_clusterActors.TryAdd(msg.ActorType, msg.ActorRef))
                _log.Info($"[ClusterManagerActor] Cluster actor '{msg.ActorType}' initialized successfully.");
            else
                _log.Error($"[ClusterManagerActor] Cluster actor '{msg.ActorType}' initialization failed.");
        }

        private IActorRef GetClusterActorHandle(GetClusterActor msg)
        {
            return _clusterActors.TryGetValue(msg.ActorType, out var actorRef) ? actorRef : ActorRefs.Nobody;
        }

        private void SendClusterActorHandle(SendClusterActor msg)
        {
            var actor = _clusterActors.TryGetValue(msg.ActorType, out var actorRef) ? actorRef : ActorRefs.Nobody;
            actor.Tell(msg.Message);
        }

        private bool RemoveClusterActorHandle(RemoveClusterActor msg)
        {
            return _clusterActors.TryRemove(msg.ActorType, out _);
        }

        public static Props Props() => Actor.Props.Create(() => new ClusterManagerActor());
    }

    #region Messages
    public record InitClusterActor(Address Address, Enum ActorType);
    public record GetClusterActor(Enum ActorType);
    public record RemoveClusterActor(Enum ActorType);
    public record SendClusterActor(Enum ActorType, object Message);

    /// <summary>
    /// `PipeTo(Self)`에서 사용될 메시지 (ActorSelection 결과 전달)
    /// </summary>
    public record ClusterActorResolved(Enum ActorType, IActorRef ActorRef);
    #endregion

    //public  class ClusterManagerActor : ReceiveActor
    //{
    //    private readonly ILoggingAdapter _log = Context.GetLogger();
    //    private readonly ConcurrentDictionary<Enum, IActorRef> _clusterActors = new();

    //    public ClusterManagerActor()
    //    {
    //        Receive<InitClusterActor>(msg => InitClusterActorHandle(msg));
    //        Receive<GetClusterActor>(msg => Sender.Tell(GetClusterActorHandle(msg)));
    //        Receive<RemoveClusterActor>(msg => Sender.Tell(RemoveClusterActorHandle(msg)));
    //        Receive<SendClusterActor>(msg => SendClusterActorHandle(msg));
    //    }

    //    private async void InitClusterActorHandle(InitClusterActor msg)
    //    {
    //        if (_clusterActors.ContainsKey(msg.ActorType))
    //            return;

    //        try
    //        {
    //            var actorSelector = Context.ActorSelection($"{msg.Address}/user/{msg.ActorType}");
    //            var actorRef = await actorSelector.ResolveOne(TimeSpan.FromSeconds(5));

    //            if (_clusterActors.TryAdd(msg.ActorType, actorRef))
    //                _log.Info($"[ClusterManagerActor] Cluster actor '{msg.ActorType}' initialized successfully.");
    //            else
    //                _log.Error($"[ClusterManagerActor] Cluster actor '{msg.ActorType}' initialization failed.");
    //        }
    //        catch (ActorNotFoundException err)
    //        {
    //            _log.Error($"[ClusterManagerActor] {msg.ActorType} Actor not found: {err.Message}");
    //        }
    //        catch (Exception ex)
    //        {
    //            _log.Error($"[ClusterManagerActor] Error initializing cluster actor '{msg.ActorType}': {ex.Message}");
    //        }
    //    }

    //    private IActorRef GetClusterActorHandle(GetClusterActor msg)
    //    {
    //        return _clusterActors.TryGetValue(msg.ActorType, out var actorRef) ? actorRef : ActorRefs.Nobody;
    //    }
    //    private void SendClusterActorHandle(SendClusterActor msg)
    //    {
    //        var actor = _clusterActors.TryGetValue(msg.ActorType, out var actorRef) ? actorRef : ActorRefs.Nobody;
    //        actor.Tell(msg.msg);
    //    }
    //    private bool RemoveClusterActorHandle(RemoveClusterActor msg)
    //    {
    //        return _clusterActors.TryRemove(msg.ActorType, out _);
    //    }

    //    public static Props Props() => Akka.Actor.Props.Create(() => new ClusterManagerActor());

    //}

    //#region Messages
    //public record InitClusterActor(Address Address, Enum ActorType);
    //public record GetClusterActor(Enum ActorType);
    //public record RemoveClusterActor(Enum ActorType);
    //public record SendClusterActor(Enum ActorType, object msg);
    //#endregion
}



//using Akka.Actor;

//using Google.Protobuf.Protocol;

//using Serilog;

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//using static Akka.Server.Define;


//namespace Akka.Server
//{
//    public class ClusterManager
//    {
//        #region Singleton
//        static readonly object _lock = new();

//        static ClusterManager _instance;
//        public static ClusterManager Instance { get { Init(); return _instance; } }
//        #endregion

//        #region Cluster
//        readonly ActorSystem _actorSystem;

//        private Dictionary<Type, IActorRef> _clusterActors = new();
//        #endregion

//        static void Init()
//        {
//            if (_instance == null)
//                lock (_lock)
//                    _instance = new ClusterManager(Program.ServerActorSystem);
//        }
//        public ClusterManager(ActorSystem actorSystem)
//        {
//            _actorSystem = actorSystem;
//        }

//        #region LogCluster
//        [Obsolete]
//        public void InitClusterActor(Address addr, Type actorName)
//        {
//            SetClusterActor(addr, actorName);
//        }
//        public void InitClusterActor(Address addr, Enum actorName)
//        {
//            _ = SetClusterActorAsync(addr, actorName);
//        }

//        private async Task SetClusterActorAsync(Address addr, Enum actorName)
//        {
//            if (_clusterActors.ContainsKey(actorName.GetType()))
//                return;

//            try
//            {
//                var actorSelector = _actorSystem.ActorSelection($"{addr}/user/{actorName}");
//                var actorRef = await actorSelector.ResolveOne(TimeSpan.FromSeconds(5));

//                lock (_lock)
//                {
//                    if (_clusterActors.TryAdd(actorName.GetType(), actorRef))
//                        Log.Logger.Information($"[ClusterManager] Cluster actor '{actorName}' initialized successfully.");
//                    else
//                        Log.Logger.Error($"[ClusterManager] Cluster actor '{actorName}' initialization failed.");
//                }
//            }
//            catch (ActorNotFoundException err)
//            {
//                Log.Logger.Error($"[ClusterManager] {actorName} Actor not found: {err.Message}");
//            }
//            catch (Exception ex)
//            {
//                Log.Logger.Error($"[ClusterManager] Error initializing cluster actor '{actorName}': {ex.Message}");
//            }
//        }
//        [Obsolete]
//        void SetClusterActor(Address addr, Type actorName)
//        {
//            if (_clusterActors.ContainsKey(actorName) == false)
//            {
//                Task.Run(async () =>
//                {
//                    try
//                    {
//                        var actorSelector = _actorSystem.ActorSelection($"{addr}/user/{actorName}");
//                        var actorRef = await actorSelector.ResolveOne(TimeSpan.FromSeconds(5));//비동기적방식
//                        //_clusterLogServer = actorSelector.ResolveOne(TimeSpan.FromSeconds(5)).Result;//동기적방식
//                        //_clusterLogServer = actorSelector.ResolveOne(TimeSpan.FromSeconds(5)).GetAwaiter().GetResult();//예외처리적으로 안전한 동기적방식

//                        lock (_lock)
//                        {
//                            if (_clusterActors.TryAdd(actorName, actorRef))
//                                Log.Logger.Information($"[ClusterManager] Cluster actor '{actorName}' initialized successfully.");
//                            else
//                                Log.Logger.Error($"[ClusterManager] Cluster actor '{actorName}' initialized Unsuccessfully.");
//                        }
//                    }
//                    catch (ActorNotFoundException err)
//                    {
//                        Log.Logger.Error($"[ClusterManager] {actorName}Actor not found: {err.Message}");
//                    }
//                    catch (Exception ex)
//                    {
//                        Log.Logger.Error($"[ClusterManager] Error initializing cluster actor '{actorName}': {ex.Message}");
//                    }
//                });
//            }
//        }
//        public IActorRef GetClusterActor(Enum key)
//        {
//            lock (_lock)
//            {
//                return _clusterActors.TryGetValue(key.GetType(), out var actorRef) ? actorRef : null;
//            }
//        }
//        public bool RemoveClusterActor(Enum key)
//        {
//            lock (_lock)
//            {
//                return _clusterActors.Remove(key.GetType(), out var actorRef);
//            }
//        }
//        #endregion
//    }
//}
