using MediatR;
using Ptcent.Cloud.Drive.Application.Contracts.Responses;

namespace Ptcent.Cloud.Drive.Application.Contracts.Requests
{
    /// <summary>
    /// 用户登录请求
    /// </summary>
    public class LoginUserRequestDto : IRequest<ResponseMessageDto<string>>
    {
        /// <summary>
        /// 手机号
        /// </summary>
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        /// 密码
        /// </summary>
        public string PassWord { get; set; } = string.Empty;
    }
}
