using Akka.Actor;

using Google.Protobuf;
using Google.Protobuf.Protocol;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class FrontendActor : ReceiveActor
    {
        public class ClusterMessage
        {
            public string Message { get; }
            public ClusterMessage(string message) => Message = message;
        }
        public FrontendActor()
        {

            Receive<byte[]>(message =>
            {
               IMessage packet =  MessageHelper.DeserializeWithType(message);

                C_Chat chat = (C_Chat)packet;
                Sender.Tell(MessageHelper.SerializeWithType(new C_Chat() { Chat = "서버메세지" }));
                Console.WriteLine($"{chat.Chat}");
            });

            Receive<string>(message =>
            {
                Console.WriteLine($"[Frontend] Received request: {message}");
                Sender.Tell($"Processed: {message}");
            });
        }
    }
}
