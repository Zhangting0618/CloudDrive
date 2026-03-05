using MediatR;
using Ptcent.Cloud.Drive.Application.Contracts.Responses;
using Ptcent.Cloud.Drive.Application.Interfaces;
using Ptcent.Cloud.Drive.Application.Services;

namespace Ptcent.Cloud.Drive.Application.Features.Users.Commands
{
    /// <summary>
    /// 用户登录命令处理器
    /// </summary>
    public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, ResponseMessageDto<string>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtService _jwtService;
        private readonly ICacheService _cacheService;

        public LoginUserCommandHandler(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IJwtService jwtService,
            ICacheService cacheService)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
            _cacheService = cacheService;
        }

        public async Task<ResponseMessageDto<string>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            var response = new ResponseMessageDto<string>();

            // 查找用户
            var user = await _userRepository.FirstOrDefaultAsync(
                u => u.Phone == request.Phone,
                cancellationToken
            );

            if (user == null)
            {
                response.IsSuccess = false;
                response.Code = WebApiResultCode.BadRequest;
                response.Message = "用户不存在";
                return response;
            }

            // 验证密码
            var isValid = _passwordHasher.VerifyPassword(request.Password, user.Salt!, user.Password);
            if (!isValid)
            {
                response.IsSuccess = false;
                response.Code = WebApiResultCode.BadRequest;
                response.Message = "密码错误";
                return response;
            }

            // 生成 JWT Token
            var token = _jwtService.GenerateToken(
                user.Id.ToString(),
                user.UserName,
                user.Phone,
                user.Email ?? string.Empty
            );

            // 缓存用户信息
            await _cacheService.SetAsync(
                $"user:session:{user.Id}",
                new { user.Id, user.UserName, user.Phone, user.Email },
                TimeSpan.FromDays(30),
                cancellationToken
            );

            response.IsSuccess = true;
            response.Code = WebApiResultCode.Success;
            response.Message = "登录成功";
            response.Data = token;

            return response;
        }
    }
}
