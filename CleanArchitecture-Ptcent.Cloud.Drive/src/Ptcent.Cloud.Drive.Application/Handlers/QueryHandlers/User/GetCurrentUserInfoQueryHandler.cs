using MediatR;
using Ptcent.Cloud.Drive.Application.Contracts.Responses;
using Ptcent.Cloud.Drive.Application.Features.Users.Queries;
using Ptcent.Cloud.Drive.Application.Interfaces.Persistence;
using Ptcent.Cloud.Drive.Domain.Enum;

namespace Ptcent.Cloud.Drive.Application.Handlers.QueryHandlers.User
{
    /// <summary>
    /// 获取当前用户信息查询处理器
    /// </summary>
    public class GetCurrentUserInfoQueryHandler : IRequestHandler<GetCurrentUserInfoQuery, ResponseMessageDto<CurrentUserDto>>
    {
        private readonly IUserRepository _userRepository;

        public GetCurrentUserInfoQueryHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<ResponseMessageDto<CurrentUserDto>> Handle(GetCurrentUserInfoQuery request, CancellationToken cancellationToken)
        {
            var response = new ResponseMessageDto<CurrentUserDto> { IsSuccess = true };

            try
            {
                // 获取当前登录用户 ID
                var userId = await _userRepository.UserId();

                // 获取用户信息
                var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
                if (user == null || user.IsDel == (int)UserStatus.Quit)
                {
                    response.IsSuccess = false;
                    response.Message = "用户不存在";
                    return response;
                }

                // 返回用户信息（不返回敏感信息如密码）
                response.Data = new CurrentUserDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Phone = user.Phone,
                    Email = user.Email,
                    Sex = user.Sex,
                    ImageUrl = user.ImageUrl,
                    RegisterTime = user.RegisterTime
                };

                return response;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = $"获取用户信息失败：{ex.Message}";
                return response;
            }
        }
    }
}
