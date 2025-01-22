using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Akka.LogServer
{

    #region Actor 이름
    public enum ActtorType
    {
        ClusterSystem,

        LogManagerActor,
        ChatLogWriteActor,
        ChatLogReaderActor,
    }
    #endregion
}
