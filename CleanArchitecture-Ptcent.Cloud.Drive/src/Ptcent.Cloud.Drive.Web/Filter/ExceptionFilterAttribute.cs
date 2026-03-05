using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Ptcent.Cloud.Drive.Application.Contracts.Responses;
using Ptcent.Cloud.Drive.Shared.Util;
using System.Text.Json;

namespace Ptcent.Cloud.Drive.Web.Filter
{
    /// <summary>
    /// 通用异常处理
    /// </summary>
    public class ExceptionFilterAttribute : IExceptionFilter
    {
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
            var requestData = GetRequestDataStr(context.HttpContext.Request);

            LogUtil.Error(
                $"请求异常：路径 {requestUrl}\r\n" +
                $"请求参数：{requestData}\r\n" +
                $"{context.Exception}");

            context.Result = new ObjectResult(res);
            context.HttpContext.Response.StatusCode = StatusCodes.Status200OK;
            context.ExceptionHandled = true;
        }

        private string GetRequestDataStr(HttpRequest request)
        {
            try
            {
                request.EnableBuffering();
                using var reader = new StreamReader(request.Body, leaveOpen: true);
                var body = reader.ReadToEndAsync().Result;
                request.Body.Position = 0;
                return body;
            }
            catch
            {
                return JsonSerializer.Serialize(request.Query);
            }
        }
    }
}
