using Microsoft.EntityFrameworkCore;
using Ptcent.Cloud.Drive.Application.Interfaces.Persistence;
using Ptcent.Cloud.Drive.Domain.Entities;
using Ptcent.Cloud.Drive.Infrastructure.Persistence;

namespace Ptcent.Cloud.Drive.Infrastructure.Repositories
{
    /// <summary>
    /// 操作日志仓储实现
    /// </summary>
    public class OperationLogRepository : Repository<OperationLogEntity>, IOperationLogRepository
    {
        public OperationLogRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<(List<OperationLogEntity> Items, int Total)> GetPagedAsync(
            long? userId,
            string? keyword,
            DateTime? startTime,
            DateTime? endTime,
            int pageIndex,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsNoTracking().AsQueryable();

            if (userId.HasValue)
            {
                query = query.Where(x => x.UserId == userId.Value);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(x =>
                    (x.Path != null && x.Path.Contains(keyword)) ||
                    (x.ActionName != null && x.ActionName.Contains(keyword)) ||
                    (x.ErrorMessage != null && x.ErrorMessage.Contains(keyword)));
            }

            if (startTime.HasValue)
            {
                query = query.Where(x => x.CreatedTime >= startTime.Value);
            }

            if (endTime.HasValue)
            {
                query = query.Where(x => x.CreatedTime <= endTime.Value);
            }

            var total = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(x => x.CreatedTime)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, total);
        }
    }
}
