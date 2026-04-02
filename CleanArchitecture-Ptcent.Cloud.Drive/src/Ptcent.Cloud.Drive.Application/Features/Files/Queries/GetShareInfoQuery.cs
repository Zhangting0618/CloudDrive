using MediatR;
using Ptcent.Cloud.Drive.Application.Contracts.Responses;

namespace Ptcent.Cloud.Drive.Application.Features.Files.Queries
{
    /// <summary>
    /// 获取分享信息查询
    /// </summary>
    public record GetShareInfoQuery(
        string ShareCode
    ) : IRequest<ResponseMessageDto<ShareInfoDto>>;

    /// <summary>
    /// 分享信息 DTO
    /// </summary>
    public class ShareInfoDto
    {
        public long FileId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public bool IsFolder { get; set; }
        public long FileSize { get; set; }
        public string? UserName { get; set; }
        public DateTime? ExpireTime { get; set; }
        public bool HasPassword { get; set; }
        public bool IsExpired { get; set; }
        public bool IsValid { get; set; }
    }
}
