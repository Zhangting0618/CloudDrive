namespace Ptcent.Cloud.Drive.Application.Services
{
    /// <summary>
    /// 文件哈希服务接口
    /// </summary>
    public interface IFileHashService
    {
        /// <summary>
        /// 计算文件哈希值（用于秒传）
        /// </summary>
        /// <param name="stream">文件流</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>文件哈希值</returns>
        Task<string> ComputeHashAsync(Stream stream, CancellationToken cancellationToken = default);

        /// <summary>
        /// 验证文件哈希
        /// </summary>
        /// <param name="stream">文件流</param>
        /// <param name="expectedHash">期望的哈希值</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>是否匹配</returns>
        Task<bool> VerifyHashAsync(Stream stream, string expectedHash, CancellationToken cancellationToken = default);
    }
}
