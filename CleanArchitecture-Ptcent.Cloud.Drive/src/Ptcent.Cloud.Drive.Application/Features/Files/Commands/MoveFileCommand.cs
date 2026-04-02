using MediatR;
using Ptcent.Cloud.Drive.Application.Contracts.Responses;

namespace Ptcent.Cloud.Drive.Application.Features.Files.Commands
{
    /// <summary>
    /// 移动文件命令
    /// </summary>
    public record MoveFileCommand(
        long FileId,
        long NewParentFolderId
    ) : IRequest<ResponseMessageDto<bool>>;
}
