using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ptcent.Cloud.Drive.Application.Interfaces;
using Ptcent.Cloud.Drive.Application.Options;
using Ptcent.Cloud.Drive.Domain.Enum;
using Ptcent.Cloud.Drive.Infrastructure.Persistence;

namespace Ptcent.Cloud.Drive.Infrastructure.Services
{
    /// <summary>
    /// 回收站清理服务
    /// </summary>
    public class RecycleBinCleanupService : IRecycleBinCleanupService
    {
        private readonly AppDbContext _dbContext;
        private readonly IOptions<RecycleBinCleanupOptions> _options;
        private readonly ILogger<RecycleBinCleanupService> _logger;

        public RecycleBinCleanupService(
            AppDbContext dbContext,
            IOptions<RecycleBinCleanupOptions> options,
            ILogger<RecycleBinCleanupService> logger)
        {
            _dbContext = dbContext;
            _options = options;
            _logger = logger;
        }

        public Task<int> CleanupExpiredAsync(CancellationToken cancellationToken = default)
        {
            var retentionDays = Math.Max(1, _options.Value.RetentionDays);
            var expireBefore = DateTime.Now.AddDays(-retentionDays);
            return DeleteDeletedFilesAsync(expireBefore, cancellationToken);
        }

        public Task<int> ClearAllAsync(CancellationToken cancellationToken = default)
        {
            return DeleteDeletedFilesAsync(null, cancellationToken);
        }

        private async Task<int> DeleteDeletedFilesAsync(DateTime? expireBefore, CancellationToken cancellationToken)
        {
            var query = _dbContext.Files
                .IgnoreQueryFilters()
                .Where(x => x.IsDel == (int)FileStatsType.Del);

            if (expireBefore.HasValue)
            {
                query = query.Where(x => x.DeletedDate != null && x.DeletedDate <= expireBefore.Value);
            }

            var deletedFiles = await query.ToListAsync(cancellationToken);
            if (deletedFiles.Count == 0)
            {
                return 0;
            }

            _dbContext.Files.RemoveRange(deletedFiles);
            var affectedRows = await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Recycle cleanup completed. DeletedRows={DeletedRows}, ExpireBefore={ExpireBefore}", affectedRows, expireBefore);
            return deletedFiles.Count;
        }
    }
}
