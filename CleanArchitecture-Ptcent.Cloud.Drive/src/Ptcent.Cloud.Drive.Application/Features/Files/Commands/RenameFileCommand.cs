using MediatR;
using Ptcent.Cloud.Drive.Application.Contracts.Responses;

namespace Ptcent.Cloud.Drive.Application.Features.Files.Commands
{
    /// <summary>
    /// 重命名文件命令
    /// </summary>
    public record RenameFileCommand(
        long FileId,
        string NewName
    ) : IRequest<ResponseMessageDto<bool>>;
}
