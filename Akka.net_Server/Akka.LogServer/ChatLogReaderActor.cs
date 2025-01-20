using Akka.Actor;
using Akka.Configuration;

using Google.Protobuf.ClusterProtocol;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Akka.LogServer
{

    public class ChatLogReaderActor : ReceiveActor
    {
        public LogConfig Config { get; private set; }

        public ChatLogReaderActor()
        {
            Receive<WriteMessage<SL_ChatReadLog>>(message =>
            {
                ChatLogLoader log = DataManager.LoadJsonLoader<ChatLogLoader>(Util.RoomNameing(message.Message.RoomId));

                LS_ChatReadLog sendLog = new LS_ChatReadLog();

                if (log != null)
                {
                    foreach (var chat in log.ChatLogLoaders)
                    {
                        sendLog.Chats.Add(chat);
                    }
                }

                message.Sender.Tell(sendLog);
            });
        }
    }
}
