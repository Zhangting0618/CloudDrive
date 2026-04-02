using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ptcent.Cloud.Drive.Application.Contracts.Requests;
using Ptcent.Cloud.Drive.Application.Contracts.Responses;
using Ptcent.Cloud.Drive.Application.Features.Users.Commands;
using Ptcent.Cloud.Drive.Application.Features.Users.Queries;

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

        public UserController(IMediator mediator)
        {
            _mediator = mediator;
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
            // TODO: 实现登出逻辑（Token 黑名单）
            return Ok(new ResponseMessageDto<bool> { IsSuccess = true, Message = "登出成功" });
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

    #endregion
}
