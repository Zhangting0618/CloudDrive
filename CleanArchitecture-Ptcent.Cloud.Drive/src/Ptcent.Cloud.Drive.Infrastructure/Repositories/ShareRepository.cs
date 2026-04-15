using Microsoft.EntityFrameworkCore;
using Ptcent.Cloud.Drive.Application.Interfaces.Persistence;
using Ptcent.Cloud.Drive.Domain.Entities;
using Ptcent.Cloud.Drive.Infrastructure.Persistence;

namespace Ptcent.Cloud.Drive.Infrastructure.Repositories
{
    public class ShareRepository : Repository<ShareEntity>, IShareRepository
    {
        public ShareRepository(AppDbContext context) : base(context)
        {
        }

        public Task<ShareEntity?> GetByShareCodeAsync(string shareCode)
        {
            return _dbSet.FirstOrDefaultAsync(x => x.ShareCode == shareCode);
        }

        public Task<bool> ExistsByShareCodeAsync(string shareCode)
        {
            return _dbSet.AsNoTracking().AnyAsync(x => x.ShareCode == shareCode);
        }
    }
}
