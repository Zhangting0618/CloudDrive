using MediatR;
using Microsoft.Extensions.Logging;
using Ptcent.Cloud.Drive.Application.Attributes;
using Ptcent.Cloud.Drive.Application.Contracts.Responses;
using Ptcent.Cloud.Drive.Application.Features.Users.Commands;
using Ptcent.Cloud.Drive.Application.Interfaces.Persistence;
using Ptcent.Cloud.Drive.Domain.Enum;

namespace Ptcent.Cloud.Drive.Application.Handlers.CommandHandlers.User
{
    /// <summary>
    /// 更新用户信息命令处理器
    /// </summary>
    [Transactional]
    public class UpdateUserInfoCommandHandler : IRequestHandler<UpdateUserInfoCommand, ResponseMessageDto<bool>>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UpdateUserInfoCommandHandler> _logger;

        public UpdateUserInfoCommandHandler(
            IUserRepository userRepository,
            ILogger<UpdateUserInfoCommandHandler> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<ResponseMessageDto<bool>> Handle(UpdateUserInfoCommand request, CancellationToken cancellationToken)
        {
            var response = new ResponseMessageDto<bool> { IsSuccess = true };

            long userId = 0;
            try
            {
                // 获取当前用户 ID
                userId = await _userRepository.UserId();

                // 获取用户信息
                var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
                if (user == null || user.IsDel == (int)UserStatus.Quit)
                {
                    response.IsSuccess = false;
                    response.Message = "用户不存在";
                    return response;
                }

                // 更新用户名（如果提供）
                if (!string.IsNullOrWhiteSpace(request.UserName))
                {
                    // 检查用户名是否已存在
                    var userNameExists = await _userRepository.AnyAsync(
                        u => u.UserName == request.UserName && u.Id != userId && u.IsDel == (int)UserStatus.Normal);

                    if (userNameExists)
                    {
                        response.IsSuccess = false;
                        response.Message = "用户名已被使用";
                        return response;
                    }

                    user.UserName = request.UserName;
                }

                // 更新邮箱（如果提供）
                if (!string.IsNullOrWhiteSpace(request.Email))
                {
                    // 简单邮箱格式验证
                    if (!request.Email.Contains("@"))
                    {
                        response.IsSuccess = false;
                        response.Message = "邮箱格式不正确";
                        return response;
                    }

                    // 检查邮箱是否已存在
                    var emailExists = await _userRepository.AnyAsync(
                        u => u.Email == request.Email && u.Id != userId && u.IsDel == (int)UserStatus.Normal);

                    if (emailExists)
                    {
                        response.IsSuccess = false;
                        response.Message = "邮箱已被使用";
                        return response;
                    }

                    user.Email = request.Email;
                }

                // 更新性别（如果提供）
                if (request.Sex.HasValue)
                {
                    if (request.Sex.Value != 0 && request.Sex.Value != 1)
                    {
                        response.IsSuccess = false;
                        response.Message = "性别参数错误";
                        return response;
                    }
                    user.Sex = request.Sex.Value;
                }

                // 更新头像（如果提供）
                if (!string.IsNullOrWhiteSpace(request.ImageUrl))
                {
                    user.ImageUrl = request.ImageUrl;
                }

                user.UpdateDate = DateTime.Now;
                await _userRepository.UpdateAsync(user, cancellationToken);

                response.Message = "更新成功";
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新用户信息失败：UserId={UserId}, Error={Error}", userId, ex.Message);

                response.IsSuccess = false;
                response.Message = $"更新失败：{ex.Message}";
                return response;
            }
        }
    }
}
