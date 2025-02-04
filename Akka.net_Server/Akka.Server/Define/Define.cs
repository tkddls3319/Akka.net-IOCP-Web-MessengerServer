using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Akka.Server
{
    public class Define
    {
        public const int RoomMaxCount = 5;

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
