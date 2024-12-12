using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ptcent.Cloud.Drive.Domain.Constants;
using Ptcent.Cloud.Drive.Domain.Entities;
using Ptcent.Cloud.Drive.Infrastructure.Context;
using Ptcent.Cloud.Drive.Infrastructure.IRespository;
using Ptcent.Cloud.Drive.Shared.Extensions;
using Ptcent.Cloud.Drive.Shared.Util;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Ptcent.Cloud.Drive.Infrastructure.Respository
{
    public class UserRepository : BaseRepository<UserEntity>, IUserRepository
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        public UserRepository(EFDbContext db, IHttpContextAccessor httpContextAccessor) : base(db)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> Phone()
        {
            var jwtJson = httpContextAccessor.HttpContext.User.FindFirst("jwt")?.Value;
            JObject jObject = JObject.Parse(jwtJson);
            string phone = (string)jObject["Phone"];
            return phone;
        }

        public async Task<long> UserId()
        {
            var jwtJson= httpContextAccessor.HttpContext.User.FindFirst("jwt")?.Value;
            JObject jObject = JObject.Parse(jwtJson);
            string userId = (string)jObject["UserId"];
            return long.Parse(userId);
        }

        public async Task<string> UserMail()
        {
            var jwtJson = httpContextAccessor.HttpContext.User.FindFirst("jwt")?.Value;
            JObject jObject = JObject.Parse(jwtJson);
            string userMail = (string)jObject["UserMail"];
            return userMail;
        }

        public async Task<string> UserName()
        {
            var jwtJson = httpContextAccessor.HttpContext.User.FindFirst("jwt")?.Value;
            JObject jObject = JObject.Parse(jwtJson);
            string userName = (string)jObject["UserName"];
            return userName;
        }
    }
}
