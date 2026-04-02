using MediatR;
using Ptcent.Cloud.Drive.Application.Contracts.Responses;

namespace Ptcent.Cloud.Drive.Application.Features.Files.Commands
{
    /// <summary>
    /// 创建分享命令
    /// </summary>
    public record CreateShareCommand(
        long FileId,
        string? AccessPassword,
        int? ExpireDays,
        int? MaxVisitCount
    ) : IRequest<ResponseMessageDto<ShareResultDto>>;

    /// <summary>
    /// 分享结果 DTO
    /// </summary>
    public class ShareResultDto
    {
        public long ShareId { get; set; }
        public string ShareCode { get; set; } = string.Empty;
        public string ShareUrl { get; set; } = string.Empty;
        public DateTime? ExpireTime { get; set; }
        public bool HasPassword { get; set; }
    }
}
