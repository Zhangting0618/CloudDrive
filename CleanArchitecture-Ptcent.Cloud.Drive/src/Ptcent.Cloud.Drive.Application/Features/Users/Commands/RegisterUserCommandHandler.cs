using MediatR;
using Ptcent.Cloud.Drive.Application.Contracts.Responses;
using Ptcent.Cloud.Drive.Application.Interfaces;
using Ptcent.Cloud.Drive.Application.Interfaces.Persistence;
using Ptcent.Cloud.Drive.Application.Services;
using Ptcent.Cloud.Drive.Domain.Entities;

namespace Ptcent.Cloud.Drive.Application.Features.Users.Commands
{
    /// <summary>
    /// 用户注册命令处理器
    /// </summary>
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, ResponseMessageDto<bool>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IIdGeneratorService _idGenerator;
        private readonly IPasswordHasher _passwordHasher;

        public RegisterUserCommandHandler(
            IUserRepository userRepository,
            IIdGeneratorService idGenerator,
            IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _idGenerator = idGenerator;
            _passwordHasher = passwordHasher;
        }

        public async Task<ResponseMessageDto<bool>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var response = new ResponseMessageDto<bool>();

            // 检查手机号是否已存在
            var phoneExists = await _userRepository.AnyAsync(
                u => u.Phone == request.Phone,
                cancellationToken
            );

            if (phoneExists)
            {
                response.IsSuccess = false;
                response.Code = WebApiResultCode.BadRequest;
                response.Message = "手机号已被注册";
                return response;
            }

            // 创建用户实体
            var userId = _idGenerator.NewId();
            var (hashedPassword, salt) = _passwordHasher.HashPassword(request.Password, request.Phone);

            var user = new UserEntity
            {
                Id = userId,
                UserName = request.UserName,
                Phone = request.Phone,
                Email = request.Email,
                Password = hashedPassword,
                Salt = salt,
                Sex = request.Sex ?? 0,
                IsDel = 0,
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow,
                CreateBy = userId,
                UpdateBy = userId,
                RegisterTime = DateTime.UtcNow
            };

            var result = await _userRepository.AddAsync(user, cancellationToken);

            response.IsSuccess = result;
            response.Code = result ? WebApiResultCode.Success : WebApiResultCode.SystemError;
            response.Message = result ? "注册成功" : "注册失败，请稍后重试";
            response.Data = result;

            return response;
        }
    }
}
