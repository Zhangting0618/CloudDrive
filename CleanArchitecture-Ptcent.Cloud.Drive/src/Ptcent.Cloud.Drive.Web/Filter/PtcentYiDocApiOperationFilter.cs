using JWT.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Ptcent.Cloud.Drive.Application.Contracts.Responses;
using Ptcent.Cloud.Drive.Application.Dto.Common;
using Ptcent.Cloud.Drive.Domain.Constants;
using Ptcent.Cloud.Drive.Domain.Enum;
using Ptcent.Cloud.Drive.Infrastructure.Cache;
using Ptcent.Cloud.Drive.Shared.Util;
using Ptcent.Cloud.Drive.Web.Controllers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApiResultCode = Ptcent.Cloud.Drive.Application.Contracts.Responses.WebApiResultCode;

namespace Ptcent.Cloud.Drive.Web.Filter
{
    /// <summary>
    /// 登录过滤器
    /// </summary>
    public class PtcentYiDocApiOperationFilter : ActionFilterAttribute
    {
        private readonly IConfiguration _config;
        private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;
        private readonly ILogger<PtcentYiDocApiOperationFilter> _logger;

        /// <summary>
        /// 构造函数
        /// </summary>
        public PtcentYiDocApiOperationFilter(
            IConfiguration config,
            JwtSecurityTokenHandler jwtSecurityTokenHandler,
            ILogger<PtcentYiDocApiOperationFilter> logger)
        {
            _config = config;
            _jwtSecurityTokenHandler = jwtSecurityTokenHandler;
            _logger = logger;
        }

        /// <summary>
        /// 在过程请求授权时调用。
        /// </summary>
        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            var response = new ResponseMessageDto<dynamic>
            {
                Data = null,
                TotalCount = 0,
                Code = (int)WebApiResultCode.SystemError,
                IsSuccess = false,
            };

            try
            {
                // 检查是否允许匿名访问（通过 [AllowAnonymous] 特性）
                var allowAnonymous = actionContext.ActionDescriptor.EndpointMetadata
                    .Any(metadata => metadata is AllowAnonymousAttribute);

                if (allowAnonymous)
                {
                    base.OnActionExecuting(actionContext);
                    return;
                }

                // 检查 Source 请求头
                if (!actionContext.HttpContext.Request.Headers.ContainsKey("Source"))
                {
                    response.Message = "请求头必须含有 source";
                    actionContext.Result = new JsonResult(response);
                    base.OnActionExecuting(actionContext);
                    return;
                }

                var controller = (BaseController)actionContext.Controller;
                controller.Source = Convert.ToInt32(actionContext.HttpContext.Request.Headers["Source"]);

                // 只处理 PC 端
                if (controller.Source != (int)Source.PC)
                {
                    response.Message = "请求头 source 错误";
                    actionContext.Result = new JsonResult(response);
                    base.OnActionExecuting(actionContext);
                    return;
                }

                // 检查 Authorization 请求头
                if (!actionContext.HttpContext.Request.Headers.ContainsKey("Authorization"))
                {
                    response.Message = "请求头需含有 Authorization";
                    actionContext.Result = new JsonResult(response);
                    base.OnActionExecuting(actionContext);
                    return;
                }

                string authToken = actionContext.HttpContext.Request.Headers["Authorization"];
                LoginUserDto loginUserDto = null;

                if (!string.IsNullOrEmpty(authToken))
                {
                    loginUserDto = GetToken(authToken);
                }

                if (loginUserDto == null)
                {
                    response.Message = "用户未登录";
                    response.Code = WebApiResultCode.NoLogin;
                    actionContext.Result = new JsonResult(response);
                    return;
                }

                // 保存当前用户信息
                controller.CurrentUserLogintDto = loginUserDto;

                // 将用户信息存入 HttpContext
                var tokenObj = new JwtSecurityToken(authToken);
                var claimsIdentity = new ClaimsIdentity(tokenObj.Claims);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                actionContext.HttpContext.User = claimsPrincipal;

                // 检查密码修改时间
                var pwdModifyTime = RedisClient.Get<DateTime>(CacheKey.Ptcent_Cloud_Drive_User_WebApi_UpdatePwd_ModifyTime_UserId + loginUserDto.UserId);
                if (loginUserDto.TokenCreateTime < pwdModifyTime)
                {
                    response.Message = "用户密码修改请重新登录";
                    response.Code = WebApiResultCode.NoLogin;
                    actionContext.Result = new JsonResult(response);
                    return;
                }

                // 检查用户登录状态
                var userLoginStatus = RedisClient.Get<string>(CacheKey.Ptcent_Cloud_Drive_WebApi_User_Login_Status + loginUserDto.UserId);
                if (userLoginStatus == UserLoginStatus.LoginOut.GetHashCode().ToString())
                {
                    response.Message = "用户未登录";
                    response.Code = WebApiResultCode.NoLogin;
                    actionContext.Result = new JsonResult(response);
                    return;
                }
            }
            catch (FormatException ex)
            {
                var requestUrl = actionContext.HttpContext.Request.Path.ToString();
                response.Message = "token 格式不对";
                actionContext.Result = new JsonResult(response);
                _logger.LogError(ex, $"请求接口异常：请求路径{requestUrl}");
            }
            catch (TokenExpiredException ex)
            {
                var requestUrl = actionContext.HttpContext.Request.Path.ToString();
                response.Message = "AuthToken 过期";
                response.Code = WebApiResultCode.NoLogin;
                actionContext.Result = new JsonResult(response);
                _logger.LogError(ex, $"请求接口异常：请求路径{requestUrl}");
            }
            catch (SignatureVerificationException ex)
            {
                var requestUrl = actionContext.HttpContext.Request.Path.ToString();
                response.Message = "AuthToken 签名无效";
                actionContext.Result = new JsonResult(response);
                _logger.LogError(ex, $"请求接口异常：请求路径{requestUrl}");
            }
            catch (Exception ex)
            {
                var requestUrl = actionContext.HttpContext.Request.Path.ToString();
                response.Message = "系统繁忙，请稍后再试！";
                response.Code = WebApiResultCode.NoLogin;
                actionContext.Result = new JsonResult(response);
                _logger.LogError(ex, $"请求接口异常：请求路径{requestUrl}");
            }

            base.OnActionExecuting(actionContext);
        }

        /// <summary>
        /// 解析 Token
        /// </summary>
        private LoginUserDto GetToken(string tokenStr)
        {
            var secretByte = Encoding.UTF8.GetBytes(_config["Authentication:SecretKey"]);
            TokenValidationParameters tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = new List<SymmetricSecurityKey>() { new SymmetricSecurityKey(secretByte) },
            };

            ClaimsPrincipal principal = _jwtSecurityTokenHandler.ValidateToken(tokenStr, tokenValidationParameters, out SecurityToken _);
            var loginUserDto = JsonConvert.DeserializeObject<LoginUserDto>(principal.Claims.FirstOrDefault()?.Value);
            return loginUserDto;
        }
    }
}
