using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Akka.Server
{
    public class Define
    {
        public const int RoomMaxCount = 100;   //하나의 룸 액터가 관리하는 세션으로 너무 많이 증가시키고 테스트하면 akka의 메세지 큐는 무한정 하지않아 메세지를 버려요.

        #region Cluster 
        public enum ClusterType
        {
            LogServer,
            AccountServer,
        }

        //해당 클러스터에서 selectactor하여 계속 사용할 액터명 정의
        public enum LogServerActorType
        {
            LogManagerActor
        }
        public enum AccountServerActorType
        {
            AccountActor
        }
        #endregion

        #region Actor 이름
        public enum ActtorType
        {
            ClusterSystem,

            clusterListenerActor,
            SessionManagerActor,
            RoomManagerActor
        }
        #endregion
    }
}
