using Google.Protobuf.ClusterProtocol;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Akka.LogServer
{
    public interface ILoader { }

    public class ChatLogLoader : ILoader
    {
        public List<ChatObject> ChatLogLoaders = new List<ChatObject>();
    }
}
