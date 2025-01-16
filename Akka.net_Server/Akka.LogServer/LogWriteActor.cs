﻿using Akka.Actor;

using Google.Protobuf;
using Google.Protobuf.Protocol;

using Serilog;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Akka.LogServer
{
    public class LogWriteActor : ReceiveActor
    {
        #region Message
        public class WriteMessage
        {
            public IActorRef Sender { get; }
            public IMessage Message { get; }

            public WriteMessage(IActorRef sender, IMessage message)
            {
                Sender = sender;
                Message = message;
            }
        }
        #endregion

        public LogWriteActor()
        {
            Receive<WriteMessage>(message =>
            {
                Log.Logger.Debug($"{Self.Path} = {message.Message}");
                //Log.Debug("This is a debug message."); // 기록됨
                //Log.Information("Application started at {Time}", DateTime.Now);
                //Log.Warning("Warning! Something might go wrong.");
                //Log.Error("An error occurred: {ErrorCode}", 404);
            });
        }
    }
}
