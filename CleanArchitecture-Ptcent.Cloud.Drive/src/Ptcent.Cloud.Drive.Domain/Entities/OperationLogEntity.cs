using System.ComponentModel.DataAnnotations;

namespace Ptcent.Cloud.Drive.Domain.Entities
{
    /// <summary>
    /// 操作日志实体
    /// </summary>
    public class OperationLogEntity
    {
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// 用户 Id，匿名操作为空
        /// </summary>
        public long? UserId { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// HTTP 方法
        /// </summary>
        public string Method { get; set; } = string.Empty;

        /// <summary>
        /// 请求路径
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// 操作名称
        /// </summary>
        public string? ActionName { get; set; }

        /// <summary>
        /// IP 地址
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// 用户代理
        /// </summary>
        public string? UserAgent { get; set; }

        /// <summary>
        /// 请求参数
        /// </summary>
        public string? RequestQuery { get; set; }

        /// <summary>
        /// HTTP 状态码
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 执行耗时（毫秒）
        /// </summary>
        public long DurationMs { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedTime { get; set; }
    }
}
