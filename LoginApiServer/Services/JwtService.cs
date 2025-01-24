using LoginApiServer.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LoginApiServer.Services
{
    public class JwtService
    {
        private IConfiguration _config;
        AppDbContext _appDbContext;

        public JwtService(IConfiguration config, AppDbContext appDbContext)
        {
            _config = config;
            _appDbContext = appDbContext;
        }

        // 사용자 아이디를 받아서 JWT 토큰생성
        public string IssueJwtToken(int accountDbId, out TimeSpan timeout)
        {
            //DateTime nowTime = DateTime.UtcNow;
            //long now = new DateTimeOffset(nowTime).ToUnixTimeSeconds();
            //long expired = new DateTimeOffset(nowTime.AddMinutes(5)).ToUnixTimeMilliseconds();
            

            DateTime now = DateTime.UtcNow;
            DateTime expired = now.AddMinutes(5);
            timeout = expired - now;

            // 클레임
            var claims = new Claim[]
            {
                // 등록된 클레임(토큰 제목, 발행 시간, 만료 시간, jwt 고유 식별자(중복방지용))
                new Claim(JwtRegisteredClaimNames.Sub, accountDbId.ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, now.ToString()),
                new Claim(JwtRegisteredClaimNames.Exp, expired.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                // 비공개 클레임
                new Claim("UserName", accountDbId.ToString())
            };

            // 비밀키
            var credential = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"])),
                SecurityAlgorithms.HmacSha256);

            // 토큰 생성
            var jwt = new JwtSecurityToken(claims: claims, signingCredentials: credential);
            string token = new JwtSecurityTokenHandler().WriteToken(jwt);

            return token;
        }
    }
}
