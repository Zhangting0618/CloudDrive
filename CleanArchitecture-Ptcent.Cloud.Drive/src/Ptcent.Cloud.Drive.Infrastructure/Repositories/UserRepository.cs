using Microsoft.AspNetCore.Http;
using Ptcent.Cloud.Drive.Application.Interfaces;
using Ptcent.Cloud.Drive.Application.Interfaces.Persistence;
using Ptcent.Cloud.Drive.Domain.Constants;
using Ptcent.Cloud.Drive.Domain.Entities;
using Ptcent.Cloud.Drive.Infrastructure.Persistence;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Ptcent.Cloud.Drive.Infrastructure.Repositories
{
    /// <summary>
    /// 用户仓储实现
    /// </summary>
    public class UserRepository : Repository<UserEntity>, IUserRepository
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserRepository(AppDbContext context, IHttpContextAccessor httpContextAccessor) : base(context)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Task<long> UserId()
        {
            var value = GetClaimValue(ClaimTypes.NameIdentifier, ClaimConst.UserId, JwtRegisteredClaimNames.Sub);
            if (!long.TryParse(value, out var userId))
            {
                throw new UnauthorizedAccessException("Current user id is unavailable.");
            }

            return Task.FromResult(userId);
        }

        public Task<string> Phone()
        {
            return Task.FromResult(GetClaimValue("Phone", ClaimConst.Phone) ?? string.Empty);
        }

        public Task<string> UserName()
        {
            return Task.FromResult(GetClaimValue(ClaimTypes.Name, ClaimConst.UserName) ?? string.Empty);
        }

        public Task<string> UserMail()
        {
            return Task.FromResult(GetClaimValue("Email", ClaimConst.UserMail) ?? string.Empty);
        }

        private string? GetClaimValue(params string[] claimTypes)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            foreach (var claimType in claimTypes)
            {
                var value = user.FindFirst(claimType)?.Value;
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            return null;
        }
    }
}
