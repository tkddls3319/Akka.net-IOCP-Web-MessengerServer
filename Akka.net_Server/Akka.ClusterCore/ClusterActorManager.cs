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
            Receive<InitClusterActorCommand>(msg => InitClusterActorHandle(msg));
            Receive<ClusterActorResolvedCommand>(msg => RegisterClusterActor(msg));
            Receive<GetClusterActorQuery>(msg => Sender.Tell(GetClusterActorHandle(msg)));
            Receive<RemoveClusterActorQuery>(msg => Sender.Tell(RemoveClusterActorHandle(msg)));
            Receive<SendClusterActorCommand>(msg => SendClusterActorHandle(msg));
        }

        /// <summary>
        /// `ActorSelection`을 사용하여 액터를 찾고 PipeTo(Self)로 전달
        /// </summary>
        private void InitClusterActorHandle(InitClusterActorCommand msg)
        {
            if (_clusterActors.ContainsKey(msg.ActorType))
                return;

            var actorSelector = Context.ActorSelection($"{msg.Address}/user/{msg.ActorType}");

            actorSelector.ResolveOne(TimeSpan.FromSeconds(5))
                .PipeTo(
                    Self,
                    success: actorRef => new ClusterActorResolvedCommand(msg.ActorType, actorRef),
                    failure: ex => new ClusterActorResolvedCommand(msg.ActorType, ActorRefs.Nobody) // 실패 시 처리
                );
        }

        /// <summary>
        /// `PipeTo(Self)`에서 받은 메시지를 처리하여 _clusterActors에 저장
        /// </summary>
        private void RegisterClusterActor(ClusterActorResolvedCommand msg)
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

        private IActorRef GetClusterActorHandle(GetClusterActorQuery msg)
        {
            return _clusterActors.TryGetValue(msg.ActorType, out var actorRef) ? actorRef : ActorRefs.Nobody;
        }

        private void SendClusterActorHandle(SendClusterActorCommand msg)
        {
            var actor = _clusterActors.TryGetValue(msg.ActorType, out var actorRef) ? actorRef : ActorRefs.Nobody;
            actor.Tell(msg.Message);
        }

        private bool RemoveClusterActorHandle(RemoveClusterActorQuery msg)
        {
            return _clusterActors.TryRemove(msg.ActorType, out _);
        }

        public static Props Props() => Actor.Props.Create(() => new ClusterManagerActor());
    }

    #region Messages
    public record InitClusterActorCommand(Address Address, Enum ActorType);
    public record GetClusterActorQuery(Enum ActorType);
    public record RemoveClusterActorQuery(Enum ActorType);
    public record SendClusterActorCommand(Enum ActorType, object Message);

    /// <summary>
    /// `PipeTo(Self)`에서 사용될 메시지 (ActorSelection 결과 전달)
    /// </summary>
    public record ClusterActorResolvedCommand(Enum ActorType, IActorRef ActorRef);
    #endregion

