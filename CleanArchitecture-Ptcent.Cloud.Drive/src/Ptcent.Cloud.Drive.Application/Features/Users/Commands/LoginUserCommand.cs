using MediatR;
using Ptcent.Cloud.Drive.Application.Contracts.Responses;

namespace Ptcent.Cloud.Drive.Application.Features.Users.Commands
{
    /// <summary>
    /// 用户登录命令
    /// </summary>
    public record LoginUserCommand(
        string Phone,
        string Password
    ) : IRequest<ResponseMessageDto<string>>;
}
