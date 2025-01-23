using Akka.AccountServer.AkkaDefine;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Akka.AccountServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AkkaController : ControllerBase
    {
        private readonly ILogger<AkkaController> _logger;
        private readonly IActorBridge _bridge;

        public AkkaController(ILogger<AkkaController> logger, IActorBridge bridge)
        {
            _logger = logger;
            _bridge = bridge;
        }

        [HttpGet]
        public Task<IEnumerable<string>> Get()
        {
            return _bridge.Ask<IEnumerable<string>>("get");
        }

        // POST api/<AkkaController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
            _bridge.Tell(value);
        }

    }
}
