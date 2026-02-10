using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Ptcent.Cloud.Drive.Application.Dto.Common;
using Ptcent.Cloud.Drive.Application.Dto.ReponseModels;
using Ptcent.Cloud.Drive.Application.ResponseMessageUntil;
using Ptcent.Cloud.Drive.Shared.Util;
using System.Net;

namespace Ptcent.Cloud.Drive.Web.Filter
{
    /// <summary>
    /// 通用异常处理  后期 扩展成中间件形式
    /// </summary>
    public class ExceptionFilterAttribute : IExceptionFilter
    {
        private readonly IResponseMessageUtil _responseUtil;

        public ExceptionFilterAttribute(IResponseMessageUtil responseUtil)
        {
            _responseUtil = responseUtil;
        }

        public void OnException(ExceptionContext context)
        {
            var res = new ResponseMessageDto<dynamic>
            {
                Data = null,
                TotalCount = 0,
                IsSuccess = false,
                Message = context.Exception.Message
            };

            var requestUrl = context.HttpContext.Request.Path;

            LogUtil.Error(
                $"请求异常：路径 {requestUrl}\r\n" +
                $"请求参数：{_responseUtil.GetHttpRequestDataStr()}\r\n" +
                $"{context.Exception}");

            context.Result = new ObjectResult(res);
            context.HttpContext.Response.StatusCode = StatusCodes.Status200OK;
            context.ExceptionHandled = true;
        }
    }
}
