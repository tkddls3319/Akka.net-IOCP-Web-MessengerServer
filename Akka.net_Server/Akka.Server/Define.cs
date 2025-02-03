using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Akka.Server
{
    public class Define
    {
        public const int RoomMaxCount = 100;

        #region Cluster 
        public enum ClusterType
        {
            LogServer,
            AccountServer,
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
