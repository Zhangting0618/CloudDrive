using MediatR;
using Ptcent.Cloud.Drive.Application.Contracts.Responses;

namespace Ptcent.Cloud.Drive.Application.Features.Files.Commands
{
    /// <summary>
    /// 取消分享命令
    /// </summary>
    public record CancelShareCommand(
        long ShareId
    ) : IRequest<ResponseMessageDto<bool>>;

    /// <summary>
    /// 获取我的分享列表查询
    /// </summary>
    public record GetMySharesQuery(
        int PageIndex = 1,
        int PageSize = 10
    ) : IRequest<ResponseMessageDto<List<MyShareDto>>>;

    /// <summary>
    /// 我的分享 DTO
    /// </summary>
    public class MyShareDto
    {
        public long ShareId { get; set; }
        public long FileId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public bool IsFolder { get; set; }
        public string ShareCode { get; set; } = string.Empty;
        public string ShareUrl { get; set; } = string.Empty;
        public DateTime? ExpireTime { get; set; }
        public bool HasPassword { get; set; }
        public int VisitCount { get; set; }
        public bool IsValid { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
