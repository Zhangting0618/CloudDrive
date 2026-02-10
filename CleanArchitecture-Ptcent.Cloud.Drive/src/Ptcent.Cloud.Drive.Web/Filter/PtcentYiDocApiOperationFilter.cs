using JWT.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Ptcent.Cloud.Drive.Application.Dto.Common;
using Ptcent.Cloud.Drive.Application.Dto.ReponseModels;
using Ptcent.Cloud.Drive.Application.ResponseMessageUntil;
using Ptcent.Cloud.Drive.Domain.Constants;
using Ptcent.Cloud.Drive.Domain.Enum;
using Ptcent.Cloud.Drive.Infrastructure.Cache;
using Ptcent.Cloud.Drive.Shared.Util;
using Ptcent.Cloud.Drive.Web.Controllers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Ptcent.Cloud.Drive.Web.Filter
{
    /// <summary>
    /// 登录过滤器
    /// </summary>
    public class PtcentYiDocApiOperationFilter : ActionFilterAttribute
    {
        private readonly IConfiguration config;
        private readonly JwtSecurityTokenHandler jwtSecurityTokenHandler;
        private readonly IResponseMessageUtil  responseMessageUtil;
        private readonly ILogger<PtcentYiDocApiOperationFilter> logger;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="config"></param>
        /// <param name="jwtSecurityTokenHandler"></param>
        /// <param name="responseMessageUtil"></param>
        public PtcentYiDocApiOperationFilter(IConfiguration config, JwtSecurityTokenHandler jwtSecurityTokenHandler, IResponseMessageUtil responseMessageUtil, ILogger<PtcentYiDocApiOperationFilter> logger)
        {
            this.config = config;
            this.jwtSecurityTokenHandler = jwtSecurityTokenHandler;
            this.responseMessageUtil = responseMessageUtil;
            this.logger = logger;
        }

        /// <summary>
        /// 在过程请求授权时调用。 
        /// </summary>
        /// <param name="actionContext">操作上下文，它封装有关使用 <see cref="T:System.Web.Http.Filters.AuthorizationFilterAttribute"/> 的信息。</param>
        public override async void OnActionExecuting(ActionExecutingContext actionContext)
        {
            var res = new ResponseMessageDto<dynamic>
            {
                Data = null,
                TotalCount = 0,
                Code = WebApiResultCode.SystemError,
                IsSuccess = false,
            };
            try
            {
                var aciontName = ((ControllerActionDescriptor)actionContext.ActionDescriptor).ActionName;
                var allowAnonymousMethodNoHeadParams = config["AllowAnonymousMethodNoHeadParam"].Split('|');
                if (allowAnonymousMethodNoHeadParams.Contains(aciontName))
                {
                    base.OnActionExecuting(actionContext);
                    return;
                }
                if (!actionContext.HttpContext.Request.Headers.ContainsKey("Source"))
                {
                    res.Message = "请求头必须含有source";
                    actionContext.Result = new JsonResult(res);
                    base.OnActionExecuting(actionContext);
                    return;
                }
                var controllerActionDescriptor = actionContext.ActionDescriptor as ControllerActionDescriptor;
                //是否允许匿名访问
                var allowAgree = controllerActionDescriptor.MethodInfo.GetCustomAttributes(typeof(AllowAnonymousAttribute), false).Any();
                if (!allowAgree)
                {
                    //Log.Info($"{DateTime.Now} ,调用方法{actionContext.RouteData.Values["controller"]}/{actionContext.RouteData.Values["action"]},参数：{actionContext.ActionArguments.ToJson()}");             
                    var baseConroller = (BaseController)actionContext.Controller;
                    if (!actionContext.HttpContext.Request.Headers.ContainsKey("AuthToken"))
                    {
                        res.Message = "请求头需含有AuthToken";
                        actionContext.Result = new JsonResult(res);
                        base.OnActionExecuting(actionContext);
                        return;
                    }
                    baseConroller.Source = Convert.ToInt32(actionContext.HttpContext.Request.Headers["Source"]);
                    //if (baseConroller.Source != (int)Source.PC)
                    //{
                    //    res.Message = "请求头source错误";
                    //    actionContext.Result = new JsonResult(res);
                    //    base.OnActionExecuting(actionContext);
                    //    return;
                    //}
                    if (baseConroller.Source == (int)Source.PC)
                    {
                        string authToken = string.Empty;
                        //#if DEBUG

                        //string authKeyJson = new BaseServiceImpl<object>().GetWxTestDebugAuthKeyJson();
                        //string authJson = authKeyJson.AESEncrypt(ConfigDto.AesKey, ConfigDto.AesIv);

                        //#else

                        authToken = actionContext.HttpContext.Request.Headers["AuthToken"];
                        //#endif
                        //Guid userId = Guid.Empty;
                        LoginUserDto loginUserDto = null;
                        if (!string.IsNullOrEmpty(authToken))
                        {
                            loginUserDto = GetToken(authToken);
                            //baseConroller.CurrentUserLogintDto = loginUserDto;
                        }
                        if (string.IsNullOrEmpty(authToken) || loginUserDto == null)
                        {
                            res.Message = "用户未登录";
                            res.Code = WebApiResultCode.NoLogin;
                            actionContext.Result = new JsonResult(res);
                            return;
                        }
                        else
                        {
                            baseConroller.CurrentUserLogintDto = loginUserDto;
                            //将用户信息存放HttpContext里面  方便取出
                            var tokenObj = new JwtSecurityToken(authToken);
                            var claimsIdentity = new ClaimsIdentity(tokenObj.Claims);
                            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                            actionContext.HttpContext.User = claimsPrincipal;
                            var pwdModifyTime = RedisClient.Get<DateTime>(CacheKey.Ptcent_Cloud_Drive_User_WebApi_UpdatePwd_ModifyTime_UserId + loginUserDto.UserId);
                            if (loginUserDto.TokenCreateTime < pwdModifyTime)
                            {
                                res.Message = "用户密码修改请重新登录";
                                res.Code = WebApiResultCode.NoLogin;
                                actionContext.Result = new JsonResult(res);
                                return;
                            }
                            var userLoginVlaue = RedisClient.Get<string>(CacheKey.Ptcent_Cloud_Drive_WebApi_User_Login_Status + loginUserDto.UserId);
                            if (userLoginVlaue == UserLoginStatus.LoginOut.GetHashCode().ToString())
                            {
                                res.Message = "用户未登录";
                                res.Code = WebApiResultCode.NoLogin;
                                actionContext.Result = new JsonResult(res);
                                return;
                            }
                        }
                    }
                    else
                    {
                        res.Message = "请求头source错误";
                        actionContext.Result = new JsonResult(res);
                        base.OnActionExecuting(actionContext);
                        return;
                    }
                }
            }
            catch (FormatException ex)
            {
                var requestUrl = actionContext.HttpContext.Request.Path.ToString();
                res.Message = "token格式不对";
                actionContext.Result = new JsonResult(res);
                LogUtil.Error($"请求接口异常：请求路劲{requestUrl}异常原因{ex}\r\n请求头：{actionContext.HttpContext.Request.Headers.ToJson()}请求参数：{responseMessageUtil.GetHttpRequestDataStr()}");
            }
            catch (TokenExpiredException ex)
            {
                var requestUrl = actionContext.HttpContext.Request.Path.ToString();
                res.Message = "AuthToken过期";
                res.Code = WebApiResultCode.NoLogin;
                actionContext.Result = new JsonResult(res);
                LogUtil.Error($"请求接口异常：请求路劲{requestUrl}异常原因{ex}\r\n请求头：{actionContext.HttpContext.Request.Headers.ToJson()}请求参数：{responseMessageUtil.GetHttpRequestDataStr()}");
            }
            catch (SignatureVerificationException ex)
            {
                var requestUrl = actionContext.HttpContext.Request.Path.ToString();
                res.Message = "AuthToken签名无效";
                actionContext.Result = new JsonResult(res);
                LogUtil.Error($"请求接口异常：请求路劲{requestUrl}异常原因{ex}\r\n请求头：{actionContext.HttpContext.Request.Headers.ToJson()}请求参数：{responseMessageUtil.GetHttpRequestDataStr()}");
            }
            catch (Exception ex)
            {
                var requestUrl = actionContext.HttpContext.Request.Path.ToString();
                res.Message = "系统繁忙，请稍后再试！";
                res.Code = WebApiResultCode.NoLogin;
                actionContext.Result = new JsonResult(res);
                LogUtil.Error($"请求接口异常：请求路劲{requestUrl}异常原因{ex}\r\n请求头：{actionContext.HttpContext.Request.Headers.ToJson()}请求参数：{responseMessageUtil.GetHttpRequestDataStr()}");
            }
            base.OnActionExecuting(actionContext);
        }
        /// <summary>
        /// 解析Token 
        /// </summary>
        /// <param name="tokenStr"></param>
        /// <returns></returns>
        private LoginUserDto GetToken(string tokenStr)
        {
            var secretByte = Encoding.UTF8.GetBytes(config["Authentication:SecretKey"]);
            TokenValidationParameters tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = new List<SymmetricSecurityKey>() { new SymmetricSecurityKey(secretByte) },
            };

            ClaimsPrincipal principal = jwtSecurityTokenHandler.ValidateToken(tokenStr, tokenValidationParameters, out SecurityToken oAuthSecurityToken);
            IEnumerable<Claim> claims = principal.Claims;
            var loginUserDto = JsonConvert.DeserializeObject<LoginUserDto>(claims.FirstOrDefault().Value);
            return loginUserDto;
        }
    }
}
