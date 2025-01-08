using Akka.Actor;
using Akka.Cluster;

using Google.Protobuf;
using Google.Protobuf.Protocol;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogServer
{
    public class BackendActor : ReceiveActor
    {
        public class ClusterMessage
        {
            public string Message { get; }
            public ClusterMessage(string message) => Message = message;
        }
        ActorSystem _actorSystem;
        public BackendActor(ActorSystem actorSystem)
        {
            _actorSystem = actorSystem; 
            Receive<string>(message =>
            {
                Console.WriteLine($"[Backend] Processing request: {message}");

                Sender.Tell(new ClusterMessage($"Backend finished processing: {message}"));
            });
            Receive<byte[]>(message =>
            {
                IMessage packet = MessageHelper.DeserializeWithType(message);

                C_Chat chat = (C_Chat)packet;
                Sender.Tell(MessageHelper.SerializeWithType(new C_Chat() { Chat = "모듈메세지" }));
                Console.WriteLine($"{chat.Chat}");
            });

        }
        protected override void PreStart()
        {
            var frontAddress = Address.Parse("akka.tcp://ClusterSystem@localhost:5000");
            var actorSelection = _actorSystem.ActorSelection($"{frontAddress}/user/Frontend");

            actorSelection.Tell(MessageHelper.SerializeWithType(new C_Chat() { Chat="Start"}));
        }
    }

}
