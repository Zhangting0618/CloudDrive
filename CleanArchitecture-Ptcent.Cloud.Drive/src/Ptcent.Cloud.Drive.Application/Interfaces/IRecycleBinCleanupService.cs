namespace Ptcent.Cloud.Drive.Application.Interfaces
{
    /// <summary>
    /// 回收站清理服务
    /// </summary>
    public interface IRecycleBinCleanupService
    {
        Task<int> CleanupExpiredAsync(CancellationToken cancellationToken = default);
        Task<int> ClearAllAsync(CancellationToken cancellationToken = default);
    }
}
