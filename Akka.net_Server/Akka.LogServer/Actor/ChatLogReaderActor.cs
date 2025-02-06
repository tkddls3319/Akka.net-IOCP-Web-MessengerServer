using Akka.Actor;
using Akka.Configuration;

using Google.Protobuf.ClusterProtocol;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Akka.LogServer
{
    public class ChatLogReaderActor : ReceiveActor
    {
        private const int SendMessageMaxSize = 128000;
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

                int objectSize = sendLog.CalculateSize();
                //Actor모델에서 메세지를 보낼 수 있는 최대크기는 128000임
                if (objectSize < SendMessageMaxSize)
                {
                    message.Sender.Tell(sendLog);
                }
                else
                {
                    message.Sender.Tell(new LS_ChatReadLogResponse());
                }
            });
        }
    }
}
