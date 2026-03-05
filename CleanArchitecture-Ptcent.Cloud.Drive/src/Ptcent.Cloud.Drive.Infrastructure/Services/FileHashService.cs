using Ptcent.Cloud.Drive.Application.Services;
using System.Security.Cryptography;

namespace Ptcent.Cloud.Drive.Infrastructure.Services
{
    /// <summary>
    /// 文件哈希服务实现
    /// </summary>
    public class FileHashService : IFileHashService
    {
        public async Task<string> ComputeHashAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            using var sha256 = SHA256.Create();

            // 确保流在开始时有正确的 position
            if (stream.CanSeek)
            {
                stream.Position = 0;
            }

            var hashBytes = await sha256.ComputeHashAsync(stream, cancellationToken);

            // 重置流位置以便后续使用
            if (stream.CanSeek)
            {
                stream.Position = 0;
            }

            return Convert.ToBase64String(hashBytes);
        }

        public async Task<bool> VerifyHashAsync(Stream stream, string expectedHash, CancellationToken cancellationToken = default)
        {
            var computedHash = await ComputeHashAsync(stream, cancellationToken);
            return computedHash == expectedHash;
        }
    }
}
