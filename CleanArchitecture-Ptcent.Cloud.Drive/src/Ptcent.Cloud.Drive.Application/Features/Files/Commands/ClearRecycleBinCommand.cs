using MediatR;
using Ptcent.Cloud.Drive.Application.Contracts.Responses;

namespace Ptcent.Cloud.Drive.Application.Features.Files.Commands
{
    /// <summary>
    /// 清空回收站命令
    /// </summary>
    public record ClearRecycleBinCommand() : IRequest<ResponseMessageDto<bool>>;
}
