using LoginApiServer.Models;
using Microsoft.EntityFrameworkCore;
using SharedDB;
using StackExchange.Redis;

namespace LoginApiServer.Services
{
    public class AccountService
    {
        AppDbContext _appDbContext;
        SharedDbContext _sharedDbContext;
        JwtService _jwtService;
        IConnectionMultiplexer _redis;

        public AccountService(AppDbContext appDbContext, SharedDbContext sharedDBContext, JwtService jwtService, IConnectionMultiplexer redisConn)
        {
            _appDbContext = appDbContext;
            _sharedDbContext = sharedDBContext;
            _jwtService = jwtService;
            _redis = redisConn;
        }

        // 클라이언트에서 로그인 요청
        public async Task<LoginAccountRes> Login(LoginAccountReq req)
        {
            LoginAccountRes res = new LoginAccountRes();

            // 계정 조회 (1.해당 계정 존재 여부, 2. 패스워드 일치 여부)
            AccountDb accountDb = _appDbContext.Accounts.AsNoTracking()
                .Where(a => a.AccountName == req.AccountName && a.AccountPw == req.AccountPw).FirstOrDefault();
            if(accountDb == null)
            {
                // 로그인 실패
                res.LoginOk = false;
            }

            // 로그인 성공
            else
            {
                res.LoginOk = true;

                // 토큰 발급
                TimeSpan timeOut;
                var tokenString = _jwtService.IssueJwtToken(accountDb.AccountDbId, out timeOut);

                Console.WriteLine(accountDb.AccountDbId);
                Console.WriteLine(timeOut.ToString());

                // 레디스에 저장
                _redis.GetDatabase().StringSet(accountDb.AccountDbId.ToString(), tokenString, timeOut);
                // _redisService.SetString(accountDb.AccountDbId.ToString(), tokenString, timeOut);
                
                // 서버 정보와 함께 응답 리턴
                res.AccountName = req.AccountName;
                res.Token = tokenString;
                res.ServerList = new List<ServerInfo>();
                foreach(ServerDb serverDb in _sharedDbContext.Servers)
                {
                    res.ServerList.Add(new ServerInfo()
                    {
                        Name = "GameServer1",//serverDb.Name,
                        IpAddress = "127.0.0.1",
                        Port = serverDb.Port,
                        BusyScore = serverDb.BusyScore,
                    });

                    //Console.Write("Check Name : "); Console.WriteLine(serverDb.Name);
                    //Console.Write("Check Ip : "); Console.WriteLine(serverDb.IpAddress);
                    //Console.Write("Check Port : "); Console.WriteLine(serverDb.Port);
                }
            }

            return res;
        }
    }


}
