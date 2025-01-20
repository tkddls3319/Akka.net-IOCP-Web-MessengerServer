using Akka.Actor;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Akka.LogServer
{
    #region Message
    public class WriteMessage<T>
    {
        public IActorRef Sender { get; }
        public T Message { get; }

        public WriteMessage(IActorRef sender, T message)
        {
            Sender = sender;
            Message = message;
        }
    }

    #endregion

}
