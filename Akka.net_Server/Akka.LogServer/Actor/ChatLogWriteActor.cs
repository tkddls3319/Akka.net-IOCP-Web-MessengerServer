using Akka.Actor;

using Google.Protobuf;
using Google.Protobuf.ClusterProtocol;
using Google.Protobuf.Protocol;

using Serilog;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Akka.LogServer
{
    public class ChatLogWriteActor : ReceiveActor
    {
        public ChatLogWriteActor()
        {
            Receive<WriteMessage<SL_ChatWriteLog>>(message =>
            {
                SL_ChatWriteLog writeLog = message.Message;

                var logger = SerilogManager.Get($"Room{writeLog.Chat.RoomId}");
                logger?.Information($"{message.Sender} - {writeLog.Chat}");
            });
        }
    }
}
