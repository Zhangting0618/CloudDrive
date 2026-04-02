using Ptcent.Cloud.Drive.Application.Interfaces;
using Ptcent.Cloud.Drive.Domain.Entities;

namespace Ptcent.Cloud.Drive.Application.Interfaces.Persistence
{
    public interface IShareRepository : IRepository<ShareEntity>
    {
        /// <summary>
        /// 根据分享码获取分享信息
        /// </summary>
        Task<ShareEntity?> GetByShareCodeAsync(string shareCode);

        /// <summary>
        /// 检查分享码是否存在
        /// </summary>
        Task<bool> ExistsByShareCodeAsync(string shareCode);
    }
}
