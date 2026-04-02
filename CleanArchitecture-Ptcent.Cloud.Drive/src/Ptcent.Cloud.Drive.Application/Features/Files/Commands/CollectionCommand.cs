using MediatR;
using Ptcent.Cloud.Drive.Application.Contracts.Responses;

namespace Ptcent.Cloud.Drive.Application.Features.Files.Commands
{
    /// <summary>
    /// 添加收藏命令
    /// </summary>
    public record AddToCollectionCommand(
        long FileId
    ) : IRequest<ResponseMessageDto<bool>>;

    /// <summary>
    /// 取消收藏命令
    /// </summary>
    public record RemoveFromCollectionCommand(
        long FileId
    ) : IRequest<ResponseMessageDto<bool>>;

    /// <summary>
    /// 获取收藏列表查询
    /// </summary>
    public record GetCollectionsQuery(
        int PageIndex = 1,
        int PageSize = 10
    ) : IRequest<ResponseMessageDto<List<CollectionItemDto>>>;

    /// <summary>
    /// 收藏项 DTO
    /// </summary>
    public class CollectionItemDto
    {
        public long CollectionId { get; set; }
        public long FileId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public bool IsFolder { get; set; }
        public long? FileSize { get; set; }
        public string? Extension { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime CollectionTime { get; set; }
    }
}