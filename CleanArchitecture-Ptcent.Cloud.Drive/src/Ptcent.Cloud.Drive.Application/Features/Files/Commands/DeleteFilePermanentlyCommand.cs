using MediatR;
using Ptcent.Cloud.Drive.Application.Contracts.Responses;

namespace Ptcent.Cloud.Drive.Application.Features.Files.Commands
{
    /// <summary>
    /// 彻底删除文件命令
    /// </summary>
    public record DeleteFilePermanentlyCommand(
        long FileId
    ) : IRequest<ResponseMessageDto<bool>>;
}
