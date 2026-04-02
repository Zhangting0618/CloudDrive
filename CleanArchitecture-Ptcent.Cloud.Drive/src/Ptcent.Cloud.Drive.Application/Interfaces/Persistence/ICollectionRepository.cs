using Ptcent.Cloud.Drive.Application.Interfaces;
using Ptcent.Cloud.Drive.Domain.Entities;

namespace Ptcent.Cloud.Drive.Application.Interfaces.Persistence
{
    public interface ICollectionRepository : IRepository<CollectionEntity>
    {
        /// <summary>
        /// 检查用户是否已收藏该文件
        /// </summary>
        Task<bool> ExistsAsync(long userId, long fileId);

        /// <summary>
        /// 获取用户的收藏列表
        /// </summary>
        Task<List<CollectionEntity>> GetUserCollectionsAsync(long userId, int pageIndex, int pageSize);

        /// <summary>
        /// 获取用户收藏总数
        /// </summary>
        Task<int> GetUserCollectionsCountAsync(long userId);
    }
}
