using System.Text.Json.Serialization;

namespace Ptcent.Cloud.Drive.Application.Contracts.Responses
{
    /// <summary>
    /// 统一响应模型
    /// </summary>
    public class ResponseMessageDto<T>
    {
        /// <summary>
        /// 状态码
        /// </summary>
        [JsonPropertyName("code")]
        public int Code { get; set; } = 200;

        /// <summary>
        /// 是否成功
        /// </summary>
        [JsonPropertyName("isSuccess")]
        public bool IsSuccess { get; set; } = true;

        /// <summary>
        /// 消息
        /// </summary>
        [JsonPropertyName("message")]
        public string? Message { get; set; }

        /// <summary>
        /// 数据
        /// </summary>
        [JsonPropertyName("data")]
        public T? Data { get; set; }

        /// <summary>
        /// 总条数（分页用）
        /// </summary>
        [JsonPropertyName("totalCount")]
        public int TotalCount { get; set; }
    }

    /// <summary>
    /// Web API 结果状态码
    /// </summary>
    public static class WebApiResultCode
    {
        public const int Success = 200;
        public const int BadRequest = 400;
        public const int Unauthorized = 401;
        public const int Forbidden = 403;
        public const int NotFound = 404;
        public const int ValidationError = 422;
        public const int SystemError = 500;
        public const int NoLogin = 401;
    }
}
