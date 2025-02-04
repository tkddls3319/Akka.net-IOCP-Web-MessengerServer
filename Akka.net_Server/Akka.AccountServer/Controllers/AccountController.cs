
using Akka.AccountServer.AkkaDefine;
using Akka.AccountServer.DB;
using Akka.AccountServer.Define;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using static Akka.AccountServer.Actor.AccountActor;

//HTTP메서드	    기능(CRUD)	    주 용도
//POST	        Create	        새로운 리소스 생성 (데이터 추가)
//GET	        Read	        리소스 조회 (데이터 검색)
//PUT       	Update	        전체 리소스 업데이트 (전체 교체)
//PATCH	        Update	        부분 리소스 업데이트 (일부 수정)
//DELETE	    Delete	        리소스 삭제 (데이터 제거)

//[FromBody] 속성은 ASP.NET Core Web API에서 클라이언트로부터 전송된 **HTTP 요청의 본문(body)**에서 데이터를 추출하는 데 사용됩니다. (json등 자동파싱)

namespace Akka.AccountServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IActorBridge _bridge;
        private readonly AppDbContext _context;
        public AccountController(AppDbContext context, ILogger<AccountController> logger, IActorBridge bridge)
        {
            _context = context;
            _logger = logger;
            _bridge = bridge;
        }

        [HttpPost]
        [Route("create")]
        public Task<CreateAccountPacketRes> CreateAccount([FromBody] CreateAccountPacketReq req)
        {
            _logger.LogInformation("[CreateAccount]");
            return _bridge.Ask<CreateAccountPacketRes>(ActtorType.AccountActor, new AccountCommand<CreateAccountPacketReq>(_context, req));
        }

        [HttpPost]
        [Route("login")]
        public Task<LoginAccountPacketRes> LoginAccount([FromBody] LoginAccountPacketReq req)
        {
            _logger.LogInformation("[LoginAccount]");
            return _bridge.Ask<LoginAccountPacketRes>(ActtorType.AccountActor, new AccountCommand<LoginAccountPacketReq>(_context, req));
        }

        int count = 100;
        [HttpGet]
        [Route("gettest")]
        public string Gettest()
        {
            return count.ToString();
        }
    }
}
