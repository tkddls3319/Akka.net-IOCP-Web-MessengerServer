using Akka.Actor;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class FrontendActor : ReceiveActor
    {
        private IActorRef _backend;

        public FrontendActor(IActorRef backend)
        {
            _backend = backend;

            Receive<string>(message =>
            {
                Console.WriteLine($"[Frontend] Received request: {message}");
                _backend.Tell($"Processed: {message}");
            });

            Receive<BackendResponse>(response =>
            {
                Console.WriteLine($"[Frontend] Response from Backend: {response.Message}");
            });
        }
    }

    public class BackendResponse
    {
        public string Message { get; }
        public BackendResponse(string message) => Message = message;
    }
}
