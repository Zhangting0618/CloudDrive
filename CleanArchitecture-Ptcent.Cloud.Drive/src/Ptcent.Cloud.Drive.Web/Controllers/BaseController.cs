using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Ptcent.Cloud.Drive.Application.Dto.Common;
using Ptcent.Cloud.Drive.Shared.Util;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Ptcent.Cloud.Drive.Web.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        private readonly IConfiguration config;
        public BaseController(IConfiguration config)
        {
            this.config = config;
        }

        public int Source { get; set; }

        /// <summary>
        /// 登录信息
        /// </summary>
        public LoginUserDto CurrentUserLogintDto { get; set; }
        /// <summary>
        /// 每页显示条数（必须传）
        /// </summary>

        public int PageSize { get; set; } = 10;
        //private readonly IHttpClientFactory clientFactory;

        public readonly static TimeSpan CacheTime = new TimeSpan(30, 0, 0, 0);

        public string ThirdTicket { get; set; }
        public readonly string FileRootPath = ConfigUtil.GetValue("FileRootPath");
        [ApiExplorerSettings(IgnoreApi = true)]
        public string CreateJwtToken(LoginUserDto loginUserDto)
        {
            var claims = new List<Claim>();
            claims.Add(new Claim("jwt", JsonConvert.SerializeObject(loginUserDto)));
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Authentication:SecretKey"]));
            var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(Convert.ToDouble(config["UserLoingExpires"] ?? "30")),
                signingCredentials: signingCredentials);
            var tokenStr = new JwtSecurityTokenHandler().WriteToken(token);
            return tokenStr;
        }
    }
}
