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
            Receive<WriteMessageCommand<SL_ChatReadLogQuery>>(message =>
            {
                ChatLogLoader log = DataManager.LoadJson<ChatLogLoader>(Util.RoomNameing(message.Message.RoomId));

                LS_ChatReadLogResponse sendLog = new LS_ChatReadLogResponse();

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
