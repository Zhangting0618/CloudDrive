using Ptcent.Cloud.Drive.Application.Dto.ReponseModels;

namespace Ptcent.Cloud.Drive.Application.Interfaces
{
    /// <summary>
    /// 存储统计服务
    /// </summary>
    public interface IStorageStatsService
    {
        Task<StorageStatsDto> GetStatsAsync(long userId, CancellationToken cancellationToken = default);
    }
}
