using System;

using Akka.Actor;
using Akka.Event;

using Serilog;

// DeadLetter 감지 Actor
public class DeadLetterMonitor : ReceiveActor
{
    public DeadLetterMonitor()
    {
        Receive<DeadLetter>(dl =>
        {
            Log.Logger.Error($"[DeadLetter 감지] Message [{dl.Message}] from [{dl.Sender}] to [{dl.Recipient}]");
        });
    }
}
