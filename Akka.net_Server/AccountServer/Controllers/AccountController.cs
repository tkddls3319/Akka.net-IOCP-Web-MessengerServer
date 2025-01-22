
using Akka.AccountServer.DB;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


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
        AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("create")]
        public CreateAccountPacketRes CreateAccount([FromBody] CreateAccountPacketReq req)
        {
            CreateAccountPacketRes res = new CreateAccountPacketRes();

            AccountDb account = _context.Accounts
                                      .AsNoTracking()//read only로 성능 향상
                                      .Where(a => a.AccountName == req.AccountName)
                                      .FirstOrDefault();

            if (account == null)
            {
                _context.Accounts.Add(new AccountDb()
                {
                    AccountName = req.AccountName,
                    Password = req.Password,
                });

                bool success = _context.SaveChangesEx();
                res.CreateOk = success;
            }
            else
            {
                res.CreateOk = false;
            }

            return res;
        }

        [HttpPost]
        [Route("login")]
        public LoginAccountPacketRes LoginAccount([FromBody] LoginAccountPacketReq req)
        {
            LoginAccountPacketRes res = new LoginAccountPacketRes();

            AccountDb account = _context.Accounts
                                    .AsNoTracking()
                                    .Where(a => a.AccountName == req.AccountName && a.Password == req.Password)
                                    .FirstOrDefault();

            if (account == null)
            {
                res.LoginOk = false;
            }
            else
            {
                res.LoginOk = true;
                //TODO : Clusetr Server에서 룸받아오기
                res.RoomList = new List<RoomInfo>()
                {
                    new RoomInfo(){Name ="1"}
                };
            }

            return res;
        }
        int count = 100;
        [HttpGet]
        [Route("getCount")]
        public string GetCount()
        {
            return count.ToString();
        }
    }
}
