using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ptcent.Cloud.Drive.Application.Contracts.Requests;
using Ptcent.Cloud.Drive.Application.Contracts.Responses;
using Ptcent.Cloud.Drive.Application.Features.Users.Commands;
using Ptcent.Cloud.Drive.Application.Features.Users.Queries;
using Ptcent.Cloud.Drive.Application.Interfaces.Persistence;
using Ptcent.Cloud.Drive.Application.Services;
using Ptcent.Cloud.Drive.Domain.Constants;
using Ptcent.Cloud.Drive.Domain.Enum;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Ptcent.Cloud.Drive.Web.Controllers
{
    /// <summary>
    /// 用户管理接口
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICacheService _cacheService;
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;

        public UserController(
            IMediator mediator,
            ICacheService cacheService,
            IUserRepository userRepository,
            IPasswordHasher passwordHasher)
        {
            _mediator = mediator;
            _cacheService = cacheService;
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        /// <summary>
        /// 用户注册
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseMessageDto<bool>>> Register([FromBody] RegistrationAccountRequestDto request)
        {
            var command = new RegisterUserCommand(
                request.UserName,
                request.Phone,
                request.PassWord,
                request.Email,
                request.Sex
            );
            return await _mediator.Send(command);
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseMessageDto<string>>> Login([FromBody] LoginUserRequestDto request)
        {
            var command = new LoginUserCommand(request.Phone, request.PassWord);
            return await _mediator.Send(command);
        }

        /// <summary>
        /// 用户登出
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult<ResponseMessageDto<bool>>> Logout()
        {
            var token = GetTokenFromRequest();
            if (!string.IsNullOrWhiteSpace(token))
            {
                var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
                var jti = jwt.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value;
                if (!string.IsNullOrWhiteSpace(jti) && jwt.ValidTo > DateTime.UtcNow)
                {
                    var expiration = jwt.ValidTo - DateTime.UtcNow;
                    var blacklistKey = string.Format(CacheKeys.UserTokenBlacklist, jti);
                    await _cacheService.SetAsync(blacklistKey, true, expiration);
                }
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrWhiteSpace(userId))
            {
                await _cacheService.RemoveAsync(string.Format(CacheKeys.UserSession, userId));
            }

            return Ok(new ResponseMessageDto<bool>
            {
                IsSuccess = true,
                Data = true,
                Message = "Logout successful"
            });
        }

        /// <summary>
        /// 获取当前用户信息
        /// </summary>
        [HttpGet("current")]
        [Authorize]
        public async Task<ActionResult<ResponseMessageDto<CurrentUserDto>>> GetCurrentUserInfo()
        {
            var query = new GetCurrentUserInfoQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// 更新用户信息
        /// </summary>
        [HttpPut("profile")]
        [Authorize]
        public async Task<ActionResult<ResponseMessageDto<bool>>> UpdateUserInfo([FromBody] UpdateUserInfoRequest request)
        {
            var command = new UpdateUserInfoCommand(request.UserName, request.Email, request.Sex, request.ImageUrl);
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        [HttpPost("change-password")]
        [Authorize]
        public async Task<ActionResult<ResponseMessageDto<bool>>> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var command = new ChangePasswordCommand(request.OldPassword, request.NewPassword);
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// 获取用户管理列表
        /// </summary>
        [HttpGet("manage/list")]
        [Authorize]
        public async Task<ActionResult<ResponseMessageDto<List<UserManagementItemDto>>>> GetUsers([FromQuery] UserManagementQueryRequest request, CancellationToken cancellationToken)
        {
            if (!await EnsureAdministratorAsync())
            {
                return Forbid();
            }

            var query = await _userRepository.QueryAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var keyword = request.Keyword.Trim();
                query = query.Where(x =>
                    x.UserName.Contains(keyword) ||
                    x.Phone.Contains(keyword) ||
                    (x.Email != null && x.Email.Contains(keyword)));
            }

            if (request.Status.HasValue)
            {
                query = query.Where(x => x.IsDel == request.Status.Value);
            }

            var total = query.Count();
            var users = query
                .OrderByDescending(x => x.RegisterTime)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return Ok(new ResponseMessageDto<List<UserManagementItemDto>>
            {
                IsSuccess = true,
                Data = users.Select(x => new UserManagementItemDto
                {
                    Id = x.Id,
                    UserName = x.UserName,
                    Phone = x.Phone,
                    Email = x.Email,
                    Sex = x.Sex,
                    ImageUrl = x.ImageUrl,
                    RegisterTime = x.RegisterTime,
                    UserType = x.UserType ?? (int)UserType.OrdinaryUsers,
                    IsAdmin = (x.UserType ?? (int)UserType.OrdinaryUsers) == (int)UserType.Administrators,
                    Status = x.IsDel ?? (int)UserStatus.Normal,
                    IsEnabled = (x.IsDel ?? (int)UserStatus.Normal) == (int)UserStatus.Normal
                }).ToList(),
                TotalCount = total,
                Message = "获取成功"
            });
        }

        /// <summary>
        /// 启用/禁用用户
        /// </summary>
        [HttpPut("manage/{userId}/status")]
        [Authorize]
        public async Task<ActionResult<ResponseMessageDto<bool>>> UpdateUserStatus(long userId, [FromBody] UpdateUserStatusRequest request, CancellationToken cancellationToken)
        {
            if (!await EnsureAdministratorAsync())
            {
                return Forbid();
            }

            var currentUserId = await _userRepository.UserId();
            if (currentUserId == userId && !request.IsEnabled)
            {
                return Ok(new ResponseMessageDto<bool>
                {
                    IsSuccess = false,
                    Data = false,
                    Message = "不能禁用当前登录管理员"
                });
            }

            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return Ok(new ResponseMessageDto<bool>
                {
                    IsSuccess = false,
                    Data = false,
                    Message = "用户不存在"
                });
            }

            user.IsDel = request.IsEnabled ? (int)UserStatus.Normal : (int)UserStatus.Quit;
            user.UpdateDate = DateTime.Now;
            user.UpdateBy = currentUserId;
            await _userRepository.UpdateAsync(user, cancellationToken);

            await _cacheService.RemoveAsync(string.Format(CacheKeys.UserSession, user.Id));

            return Ok(new ResponseMessageDto<bool>
            {
                IsSuccess = true,
                Data = true,
                Message = request.IsEnabled ? "用户已启用" : "用户已禁用"
            });
        }

        /// <summary>
        /// 管理员重置用户密码
        /// </summary>
        [HttpPost("manage/{userId}/reset-password")]
        [Authorize]
        public async Task<ActionResult<ResponseMessageDto<bool>>> ResetUserPassword(long userId, [FromBody] AdminResetPasswordRequest request, CancellationToken cancellationToken)
        {
            if (!await EnsureAdministratorAsync())
            {
                return Forbid();
            }

            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return Ok(new ResponseMessageDto<bool>
                {
                    IsSuccess = false,
                    Data = false,
                    Message = "用户不存在"
                });
            }

            var newPassword = string.IsNullOrWhiteSpace(request.NewPassword) ? "123456" : request.NewPassword.Trim();
            if (newPassword.Length < 6 || newPassword.Length > 20)
            {
                return Ok(new ResponseMessageDto<bool>
                {
                    IsSuccess = false,
                    Data = false,
                    Message = "密码长度必须在 6 到 20 位之间"
                });
            }

            var (hashedPassword, salt) = _passwordHasher.HashPassword(newPassword, user.Phone);
            user.Password = hashedPassword;
            user.Salt = salt;
            user.UpdateDate = DateTime.Now;
            user.UpdateBy = await _userRepository.UserId();
            await _userRepository.UpdateAsync(user, cancellationToken);

            await _cacheService.RemoveAsync(string.Format(CacheKeys.UserSession, user.Id));

            return Ok(new ResponseMessageDto<bool>
            {
                IsSuccess = true,
                Data = true,
                Message = $"密码已重置为：{newPassword}"
            });
        }

        private string? GetTokenFromRequest()
        {
            var authorization = Request.Headers.Authorization.ToString();
            if (string.IsNullOrWhiteSpace(authorization))
            {
                return null;
            }

            return authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                ? authorization["Bearer ".Length..].Trim()
                : authorization.Trim();
        }

        private async Task<bool> EnsureAdministratorAsync()
        {
            var userId = await _userRepository.UserId();
            var user = await _userRepository.GetByIdAsync(userId);
            return user != null &&
                user.IsDel == (int)UserStatus.Normal &&
                (user.UserType ?? (int)UserType.OrdinaryUsers) == (int)UserType.Administrators;
        }
    }

    #region 请求 DTO

    public class UpdateUserInfoRequest
    {
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public int? Sex { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class ChangePasswordRequest
    {
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class UserManagementQueryRequest
    {
        public string? Keyword { get; set; }
        public int? Status { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class UpdateUserStatusRequest
    {
        public bool IsEnabled { get; set; }
    }

    public class AdminResetPasswordRequest
    {
        public string? NewPassword { get; set; }
    }

    public class UserManagementItemDto
    {
        public long Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public int? Sex { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime? RegisterTime { get; set; }
        public int UserType { get; set; }
        public bool IsAdmin { get; set; }
        public int Status { get; set; }
        public bool IsEnabled { get; set; }
    }

    #endregion
}
