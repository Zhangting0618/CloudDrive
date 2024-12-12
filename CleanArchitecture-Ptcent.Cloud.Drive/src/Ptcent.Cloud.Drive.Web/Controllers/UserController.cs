using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ptcent.Cloud.Drive.Application.Dto.ReponseModels;
using Ptcent.Cloud.Drive.Application.Dto.RequestModels;
using Ptcent.Cloud.Drive.Shared.Extensions;
using Ptcent.Cloud.Drive.Shared.Util;
using System.Web;

namespace Ptcent.Cloud.Drive.Web.Controllers
{
    /// <summary>
    /// 用户
    /// </summary>
    public class UserController : BaseController
    {
        private readonly IConfiguration config;
        private readonly IMediator mediator;
        private readonly IHttpClientFactory clientFactory;
        /// <summary>
        /// 用户模块
        /// </summary>
        /// <param name="config"></param>
        /// <param name="mediator"></param>
        public UserController(IConfiguration config, IMediator mediator, IHttpClientFactory clientFactory) : base(config)
        {
            this.config = config;
            this.mediator = mediator;
            this.clientFactory = clientFactory;
        }
        /// <summary>
        /// 注册用户
        /// </summary>
        /// <param name="registrationAccountRequestDto"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<ResponseMessageDto<bool>> RegistrationAccount(RegistrationAccountRequestDto registrationAccountRequestDto)
        {
            return await mediator.Send(registrationAccountRequestDto);
        }
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="loginUserRequestDto"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<ResponseMessageDto<string>> Login(LoginUserRequestDto loginUserRequestDto)
        {
            return await mediator.Send(loginUserRequestDto);
        }
        /// <summary>
        /// 退出
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ResponseMessageDto<bool>> SignOut(long userId)=>await mediator.Send(new UserQueryRequestDto { UserId = userId });
        /// <summary>
        /// 获取微信登录二维码
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GenerateCode()
        {
            string sign = Guid.NewGuid().ToString().Replace("-", string.Empty);
            string callBackUrl = "https://v5698k5455.vicp.fun/cloudapi/User/WxcallBack";
            callBackUrl= HttpUtility.UrlEncode(callBackUrl);
            //https://open.weixin.qq.com/connect/oauth2/authorize?appid=APPID&redirect_uri=REDIRECT_URI&response_type=code&scope=SCOPE&state=STATE#wechat_redirec
            string url = $"https://open.weixin.qq.com/connect/oauth2/authorize?appid=wx0739d9488f396ac8&redirect_uri={callBackUrl}&response_type=code&scope=snsapi_userinfo&state=STATE#wechat_redirect";//微信地址
          var bytes=  QRCodeUtil.GenerateQrCodeBase64(url);
            return File(bytes, "image/jpeg");
        }
        /// <summary>
        /// 微信登录验证
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<string> WxCheck()
        {
            //string signature = HttpContext.Request.Headers["signature"];
            string signature = HttpContext.Request.Query["signature"];
            string timestamp = HttpContext.Request.Query["timestamp"];
            string nonce = HttpContext.Request.Query["nonce"];
            string echostr = HttpContext.Request.Query["echostr"];
            if (echostr.IsNullOrWhiteSpace())
            {
                LogUtil.Info("获取微信验证参数为空");
                //http://localhost:5243/cloudapi/User/WxCheck?signature=43b43101c1549163d2b113e0e20bb8dc3c4eb1df&echostr=355231314065480533&timestamp=1710332181&nonce=166908108
            }
            LogUtil.Info("获取微信验证参数:" + echostr);
            return echostr;
        }
        /// <summary>
        /// 微信回调接口
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task <string> WxcallBack()
        {
            string code = HttpContext.Request.Query["code"];
            string state = HttpContext.Request.Query["state"];
            LogUtil.Info("WxcallBack:code:" + code + ",state:" + state);
            var client = clientFactory.CreateClient();
            //https://api.weixin.qq.com/sns/oauth2/access_token?appid=APPID&secret=SECRET&code=CODE&grant_type=authorization_code
            string appID = "wx0739d9488f396ac8";
            string appsecret = "b81e5f321f4d46fc33767b4232fc1fe3";
            string getTokenUrl = $"https://api.weixin.qq.com/sns/oauth2/access_token?appid={appID}&secret={appsecret}&code={code}&grant_type=authorization_code";
           var result=  await  client.GetAsync(getTokenUrl);
           var s=await result.Content.ReadAsStringAsync();
            LogUtil.Info(s);
            return s;
        }
    }
}
