using Ptcent.Cloud.Drive.Domain.Entities;

namespace Ptcent.Cloud.Drive.Application.Interfaces.Persistence
{
    public interface IOperationLogRepository : IRepository<OperationLogEntity>
    {
        Task<(List<OperationLogEntity> Items, int Total)> GetPagedAsync(
            long? userId,
            string? keyword,
            DateTime? startTime,
            DateTime? endTime,
            int pageIndex,
            int pageSize,
            CancellationToken cancellationToken = default);
    }
}
