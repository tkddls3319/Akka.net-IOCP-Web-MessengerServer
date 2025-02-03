using Akka.Actor;
using Akka.Cluster;
using Akka.Event;


namespace Akka.ClusterCore
{
    /// <summary>
    /// Define정의안하고 ClusterManager를 사용하지 않고 하고 싶다면 Cluster를 찾아 ActorSelection을 자동화 하기 위한 Class
    /// </summary>
    public class ClusterActorLocator
    {
        private readonly ActorSystem _actorSystem;
        private readonly Cluster.Cluster _cluster;
        private readonly ILoggingAdapter _log;

        public ClusterActorLocator(ActorSystem actorSystem)
        {
            _actorSystem = actorSystem;
            _cluster = Cluster.Cluster.Get(actorSystem);
            _log = Logging.GetLogger(actorSystem, this);
        }

        public async Task<T> AskClusterActor<T>(string actorName, object message, TimeSpan timeout)
        {
            var clusterState = _cluster.State;

            // 특정Actor가 있는 노드를 찾음
            var targetNode = await FindRoomManagerActor(actorName, timeout);

            if (targetNode == null)
            {
                _log.Warning("클러스터에서 사용할 수 있는 노드가 없습니다.");
                return default;
            }

            try
            {
                return await targetNode.Ask<T>(message, timeout);
            }
            catch (Exception ex)
            {
                return default;
            }
        }
        public async Task<IActorRef> FindRoomManagerActor(string actorName, TimeSpan timeout)
        {
            var clusterState = _cluster.State;

            // 모든 활성화된 노드의 주소 가져오기
            var activeNodes = clusterState.Members
                                          .Where(m => m.Status == MemberStatus.Up)
                                          .Select(m => m.Address)
                                          .ToList();

            foreach (var node in activeNodes)
            {
                var actorPath = $"{node}/user/{actorName}";
                var actorSelection = _actorSystem.ActorSelection(actorPath);

                // Identify 메시지를 보내 해당 액터가 존재하는지 확인
                var identifyResponse = await actorSelection.Ask<ActorIdentity>(new Identify(null), timeout);

                if (identifyResponse.Subject != null) // 특정액터가 존재하는 경우
                {
                    return identifyResponse.Subject;
                }
            }

            return null;
        }

        public async Task<T> AskClusterActorByRole<T>(string role, string actorName, object message, TimeSpan timeout)
        {
            var clusterState = _cluster.State;

            // 특정 역할을 가진 노드에서만 찾기
            var targetNode = clusterState.Members
                                         .Where(m => m.Status == MemberStatus.Up && m.Roles.Contains(role))
                                         .Select(m => m.Address)
                                         .FirstOrDefault();

            if (targetNode == null)
            {
                _log.Warning($"[ClusterActorLocator] '{role}' 역할을 가진 사용 가능한 노드가 없습니다.");
                return default;
            }

            var actorPath = $"{targetNode}/user/{actorName}";

            _log.Info($"[ClusterActorLocator] '{role}' 역할을 가진 노드에서 찾은 액터: {actorPath}");

            try
            {
                var actorSelection = _actorSystem.ActorSelection(actorPath);
                return await actorSelection.Ask<T>(message, timeout);
            }
            catch (Exception ex)
            {
                _log.Error(ex, $"[ClusterActorLocator] 액터 {actorPath}에 요청 실패");
                return default;
            }
        }
    }
}
