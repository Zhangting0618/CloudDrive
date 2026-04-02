using MediatR;
using Ptcent.Cloud.Drive.Application.Contracts.Responses;
using Ptcent.Cloud.Drive.Application.Dto.ReponseModels;

namespace Ptcent.Cloud.Drive.Application.Features.Files.Queries
{
    /// <summary>
    /// 回收站列表查询
    /// </summary>
    public record RecycleBinQuery(
        int PageIndex = 1,
        int PageSize = 10
    ) : IRequest<ResponseMessageDto<List<ItemResponseDto>>>;
}
