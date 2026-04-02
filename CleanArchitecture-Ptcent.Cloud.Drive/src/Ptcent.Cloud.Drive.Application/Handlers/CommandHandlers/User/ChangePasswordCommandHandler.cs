using MediatR;
using Microsoft.Extensions.Logging;
using Ptcent.Cloud.Drive.Application.Attributes;
using Ptcent.Cloud.Drive.Application.Contracts.Responses;
using Ptcent.Cloud.Drive.Application.Features.Users.Commands;
using Ptcent.Cloud.Drive.Application.Interfaces;
using Ptcent.Cloud.Drive.Application.Interfaces.Persistence;
using Ptcent.Cloud.Drive.Domain.Enum;
using System.Security.Cryptography;
using System.Text;

namespace Ptcent.Cloud.Drive.Application.Handlers.CommandHandlers.User
{
    /// <summary>
    /// 修改密码命令处理器
    /// </summary>
    [Transactional]
    public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, ResponseMessageDto<bool>>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<ChangePasswordCommandHandler> _logger;

        public ChangePasswordCommandHandler(
            IUserRepository userRepository,
            ILogger<ChangePasswordCommandHandler> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<ResponseMessageDto<bool>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
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

                // 验证原密码
                var oldPasswordHash = HashPassword(request.OldPassword, user.Salt);
                if (oldPasswordHash != user.Password)
                {
                    response.IsSuccess = false;
                    response.Message = "原密码错误";
                    return response;
                }

                // 验证新密码
                if (string.IsNullOrWhiteSpace(request.NewPassword))
                {
                    response.IsSuccess = false;
                    response.Message = "新密码不能为空";
                    return response;
                }

                if (request.NewPassword.Length < 6)
                {
                    response.IsSuccess = false;
                    response.Message = "密码长度不能少于 6 位";
                    return response;
                }

                if (request.NewPassword.Length > 20)
                {
                    response.IsSuccess = false;
                    response.Message = "密码长度不能超过 20 位";
                    return response;
                }

                // 生成新盐值并加密新密码
                var newSalt = GenerateSalt();
                var newPasswordHash = HashPassword(request.NewPassword, newSalt);

                user.Password = newPasswordHash;
                user.Salt = newSalt;
                user.UpdateDate = DateTime.Now;

                await _userRepository.UpdateAsync(user, cancellationToken);

                response.Message = "密码修改成功，请重新登录";
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "修改密码失败：UserId={UserId}, Error={Error}", userId, ex.Message);

                response.IsSuccess = false;
                response.Message = $"修改失败：{ex.Message}";
                return response;
            }
        }

        /// <summary>
        /// 生成盐值
        /// </summary>
        private string GenerateSalt()
        {
            var bytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// 密码哈希（SHA256）
        /// </summary>
        private string HashPassword(string password, string salt)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password + salt);
            var hashBytes = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hashBytes);
        }
    }
}
