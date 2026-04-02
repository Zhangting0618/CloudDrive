using MediatR;
using Ptcent.Cloud.Drive.Application.Contracts.Responses;

namespace Ptcent.Cloud.Drive.Application.Features.Users.Commands
{
    /// <summary>
    /// 修改密码命令
    /// </summary>
    public record ChangePasswordCommand(
        string OldPassword,
        string NewPassword
    ) : IRequest<ResponseMessageDto<bool>>;
}
