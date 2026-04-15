using Microsoft.EntityFrameworkCore;
using Ptcent.Cloud.Drive.Application.Dto.ReponseModels;
using Ptcent.Cloud.Drive.Application.Interfaces;
using Ptcent.Cloud.Drive.Domain.Enum;
using Ptcent.Cloud.Drive.Infrastructure.Persistence;

namespace Ptcent.Cloud.Drive.Infrastructure.Services
{
    /// <summary>
    /// 存储统计服务
    /// </summary>
    public class StorageStatsService : IStorageStatsService
    {
        private readonly AppDbContext _dbContext;

        public StorageStatsService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<StorageStatsDto> GetStatsAsync(long userId, CancellationToken cancellationToken = default)
        {
            var allFiles = _dbContext.Files.IgnoreQueryFilters().AsNoTracking();

            var userActiveFiles = allFiles.Where(x => x.CreatedBy == userId && x.IsDel == (int)FileStatsType.NoDel);
            var userRecycleFiles = allFiles.Where(x => x.CreatedBy == userId && x.IsDel == (int)FileStatsType.Del);
            var systemActiveFiles = allFiles.Where(x => x.IsDel == (int)FileStatsType.NoDel);
            var systemRecycleFiles = allFiles.Where(x => x.IsDel == (int)FileStatsType.Del);

            return new StorageStatsDto
            {
                UserUsedBytes = await userActiveFiles.Where(x => x.IsFolder != 1).SumAsync(x => (long?)x.FileSize, cancellationToken) ?? 0,
                UserRecycleBytes = await userRecycleFiles.Where(x => x.IsFolder != 1).SumAsync(x => (long?)x.FileSize, cancellationToken) ?? 0,
                UserFileCount = await userActiveFiles.CountAsync(x => x.IsFolder != 1, cancellationToken),
                UserFolderCount = await userActiveFiles.CountAsync(x => x.IsFolder == 1, cancellationToken),
                SystemUsedBytes = await systemActiveFiles.Where(x => x.IsFolder != 1).SumAsync(x => (long?)x.FileSize, cancellationToken) ?? 0,
                SystemRecycleBytes = await systemRecycleFiles.Where(x => x.IsFolder != 1).SumAsync(x => (long?)x.FileSize, cancellationToken) ?? 0,
                SystemFileCount = await systemActiveFiles.CountAsync(x => x.IsFolder != 1, cancellationToken),
                SystemFolderCount = await systemActiveFiles.CountAsync(x => x.IsFolder == 1, cancellationToken)
            };
        }
    }
}
