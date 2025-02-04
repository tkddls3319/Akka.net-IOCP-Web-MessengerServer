using Akka.Actor;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Akka.LogServer
{
    #region Message
    public class WriteMessageCommand<T>
    {
        public IActorRef Sender { get; }
        public T Message { get; }

        public WriteMessageCommand(IActorRef sender, T message)
        {
            Sender = sender;
            Message = message;
        }
    }
    #endregion
}
