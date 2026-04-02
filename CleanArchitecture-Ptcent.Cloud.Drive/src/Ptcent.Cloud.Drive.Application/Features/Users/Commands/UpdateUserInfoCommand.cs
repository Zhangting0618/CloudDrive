using MediatR;
using Ptcent.Cloud.Drive.Application.Contracts.Responses;

namespace Ptcent.Cloud.Drive.Application.Features.Users.Commands
{
    /// <summary>
    /// 更新用户信息命令
    /// </summary>
    public record UpdateUserInfoCommand(
        string? UserName,
        string? Email,
        int? Sex,
        string? ImageUrl
    ) : IRequest<ResponseMessageDto<bool>>;
}
