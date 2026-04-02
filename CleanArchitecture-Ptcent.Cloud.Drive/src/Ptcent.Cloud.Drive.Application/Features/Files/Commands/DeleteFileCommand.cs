using MediatR;
using Ptcent.Cloud.Drive.Application.Contracts.Responses;

namespace Ptcent.Cloud.Drive.Application.Features.Files.Commands
{
    /// <summary>
    /// 删除文件命令
    /// </summary>
    public record DeleteFileCommand(
        long FileId
    ) : IRequest<ResponseMessageDto<bool>>;
}
