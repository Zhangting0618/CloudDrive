using Ptcent.Cloud.Drive.Domain.Entities;

namespace Ptcent.Cloud.Drive.Application.Interfaces
{
    /// <summary>
    /// 文件仓储接口
    /// </summary>
    public interface IFileRepository : IRepository<FileEntity>
    {
        /// <summary>
        /// 根据父文件夹 ID 获取文件列表
        /// </summary>
        Task<List<FileEntity>> GetByParentFolderIdAsync(long parentFolderId, CancellationToken cancellationToken = default);

        /// <summary>
        /// 根据 ID 路径获取文件
        /// </summary>
        Task<FileEntity?> GetByIdpathAsync(string idpath, CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取用户的所有文件
        /// </summary>
        Task<List<FileEntity>> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default);
    }
}
