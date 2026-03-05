using Ptcent.Cloud.Drive.Application.Interfaces;
using Ptcent.Cloud.Drive.Application.Interfaces.Persistence;
using Ptcent.Cloud.Drive.Domain.Entities;
using Ptcent.Cloud.Drive.Infrastructure.Persistence;

namespace Ptcent.Cloud.Drive.Infrastructure.Repositories
{
    /// <summary>
    /// 用户仓储实现
    /// </summary>
    public class UserRepository : Repository<UserEntity>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<long> UserId()
        {
            // TODO: 从当前用户获取
            return await Task.FromResult(0L);
        }

        public async Task<string> Phone()
        {
            // TODO: 从当前用户获取
            return await Task.FromResult(string.Empty);
        }

        public async Task<string> UserName()
        {
            // TODO: 从当前用户获取
            return await Task.FromResult(string.Empty);
        }

        public async Task<string> UserMail()
        {
            // TODO: 从当前用户获取
            return await Task.FromResult(string.Empty);
        }
    }
}
