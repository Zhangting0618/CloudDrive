using System.ComponentModel.DataAnnotations;

namespace Ptcent.Cloud.Drive.Domain.Entities
{
    /// <summary>
    /// 用户收藏实体
    /// </summary>
    public class CollectionEntity
    {
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// 用户 ID
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 文件 ID
        /// </summary>
        public long FileId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
