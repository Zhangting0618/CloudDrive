namespace Ptcent.Cloud.Drive.Application.Dto.ReponseModels
{
    /// <summary>
    /// 操作日志响应 DTO
    /// </summary>
    public class OperationLogItemDto
    {
        public long Id { get; set; }
        public long? UserId { get; set; }
        public string? UserName { get; set; }
        public string? Phone { get; set; }
        public string Method { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string? ActionName { get; set; }
        public string? IpAddress { get; set; }
        public string? RequestQuery { get; set; }
        public int StatusCode { get; set; }
        public bool IsSuccess { get; set; }
        public long DurationMs { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}
