using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Akka.Server
{
    public class Define
    {
        #region Cluster 
        public static readonly string AddrLogManagerActor = "akka.tcp://ClusterSystem@localhost:5001";
        public enum ClusterType
        {
            LogManagerActor,
        }
        #endregion

        public const int RoomMaxCount = 100;
    }
}
