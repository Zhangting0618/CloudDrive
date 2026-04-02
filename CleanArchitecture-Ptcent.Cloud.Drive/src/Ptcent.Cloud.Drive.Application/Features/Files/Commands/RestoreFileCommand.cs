using MediatR;
using Ptcent.Cloud.Drive.Application.Contracts.Responses;

namespace Ptcent.Cloud.Drive.Application.Features.Files.Commands
{
    /// <summary>
    /// 还原文件命令
    /// </summary>
    public record RestoreFileCommand(
        long FileId
    ) : IRequest<ResponseMessageDto<bool>>;
}
