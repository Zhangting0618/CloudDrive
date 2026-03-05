using System.ComponentModel.DataAnnotations;

namespace Ptcent.Cloud.Drive.Domain.Entities
{
    /// <summary>
    /// 用户实体
    /// </summary>
    public class UserEntity
    {
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// 手机号
        /// </summary>
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        /// 密码（加密存储）
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// 性别 0-男 1-女
        /// </summary>
        public int? Sex { get; set; }

        /// <summary>
        /// 是否删除 0-否 1-是
        /// </summary>
        public int? IsDel { get; set; }

        /// <summary>
        /// 头像地址
        /// </summary>
        public string? ImageUrl { get; set; }

        /// <summary>
        /// 注册时间
        /// </summary>
        public DateTime? RegisterTime { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateDate { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdateDate { get; set; }

        /// <summary>
        /// 盐值
        /// </summary>
        public string? Salt { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public long? CreateBy { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        public long? UpdateBy { get; set; }
    }
}
