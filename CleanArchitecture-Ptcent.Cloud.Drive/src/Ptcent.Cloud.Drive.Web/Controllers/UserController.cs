using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ptcent.Cloud.Drive.Application.Contracts.Requests;
using Ptcent.Cloud.Drive.Application.Contracts.Responses;
using Ptcent.Cloud.Drive.Application.Features.Users.Commands;

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
            // TODO: 实现登出逻辑
            return Ok(new ResponseMessageDto<bool> { IsSuccess = true, Message = "登出成功" });
        }
    }
}
