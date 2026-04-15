using MediatR;
using Ptcent.Cloud.Drive.Application.Contracts.Responses;

namespace Ptcent.Cloud.Drive.Application.Features.Files.Commands
{
    /// <summary>
    /// 复制文件/文件夹命令
    /// </summary>
    public record CopyFileCommand(
        long FileId,
        long? TargetParentFolderId
    ) : IRequest<ResponseMessageDto<bool>>;
}
