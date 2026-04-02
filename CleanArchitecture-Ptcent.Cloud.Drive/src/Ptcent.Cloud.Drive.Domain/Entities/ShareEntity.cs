using System.ComponentModel.DataAnnotations;

namespace Ptcent.Cloud.Drive.Domain.Entities
{
    /// <summary>
    /// 文件分享实体
    /// </summary>
    public class ShareEntity
    {
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// 分享码（唯一标识）
        /// </summary>
        public string ShareCode { get; set; } = string.Empty;

        /// <summary>
        /// 文件 ID
        /// </summary>
        public long FileId { get; set; }

        /// <summary>
        /// 文件路径 ID（用于文件夹分享）
        /// </summary>
        public string? FileIdPath { get; set; }

        /// <summary>
        /// 分享人 ID
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 访问密码（可选）
        /// </summary>
        public string? AccessPassword { get; set; }

        /// <summary>
        /// 过期时间（null 表示永久有效）
        /// </summary>
        public DateTime? ExpireTime { get; set; }

        /// <summary>
        /// 访问次数
        /// </summary>
        public int VisitCount { get; set; }

        /// <summary>
        /// 最大访问次数（0 表示无限制）
        /// </summary>
        public int MaxVisitCount { get; set; }

        /// <summary>
        /// 是否有效
        /// </summary>
        public int IsValid { get; set; } = 1;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }
    }
}
