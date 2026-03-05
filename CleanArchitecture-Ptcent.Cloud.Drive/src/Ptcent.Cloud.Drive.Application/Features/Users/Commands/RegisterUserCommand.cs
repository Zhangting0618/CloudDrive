using MediatR;
using Ptcent.Cloud.Drive.Application.Contracts.Responses;

namespace Ptcent.Cloud.Drive.Application.Features.Users.Commands
{
    /// <summary>
    /// 用户注册命令
    /// </summary>
    public record RegisterUserCommand(
        string UserName,
        string Phone,
        string Password,
        string Email,
        int? Sex
    ) : IRequest<ResponseMessageDto<bool>>;
}
