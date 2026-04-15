using Microsoft.EntityFrameworkCore;
using Ptcent.Cloud.Drive.Application.Interfaces.Persistence;
using Ptcent.Cloud.Drive.Domain.Entities;
using Ptcent.Cloud.Drive.Infrastructure.Persistence;

namespace Ptcent.Cloud.Drive.Infrastructure.Repositories
{
    public class CollectionRepository : Repository<CollectionEntity>, ICollectionRepository
    {
        public CollectionRepository(AppDbContext context) : base(context)
        {
        }

        public Task<bool> ExistsAsync(long userId, long fileId)
        {
            return _dbSet.AsNoTracking().AnyAsync(x => x.UserId == userId && x.FileId == fileId);
        }

        public Task<List<CollectionEntity>> GetUserCollectionsAsync(long userId, int pageIndex, int pageSize)
        {
            return _dbSet
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreateTime)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public Task<int> GetUserCollectionsCountAsync(long userId)
        {
            return _dbSet.AsNoTracking().CountAsync(x => x.UserId == userId);
        }
    }
}
