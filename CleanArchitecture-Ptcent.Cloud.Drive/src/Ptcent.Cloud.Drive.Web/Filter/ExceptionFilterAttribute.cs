using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Ptcent.Cloud.Drive.Application.Dto.ReponseModels;
using Ptcent.Cloud.Drive.Shared.Util;
using Ptcent.Cloud.Drive.Application.Dto.Common;

namespace Ptcent.Cloud.Drive.Web.Filter
{
    /// <summary>
    /// 通用异常处理  后期 扩展成中间件形式
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
                //Message = string.Format("{0}参数错误或为空", context?.Exception?.Message)
            };
            try
            {
                var requestUrl = context.HttpContext.Request.Path.ToString();
                //Log.Error($"请求异常：{context.Exception}\r\n请求参数：{UtilResponseMessage.GetRequestDataStr()}\r\n用户信息：{UtilResponseMessage.GetCurrentUserDto()?.ToJson()}");
                LogUtil.Error($"请求异常：路径{requestUrl} {context.Exception}\r\n请求参数：{UtilResponseMessage.GetHttpRequestDataStr()}");
                // res.Message = "系统繁忙，请稍后再试！";
                res.Message = context.Exception.Message;
                context.Result = new ObjectResult(res);
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
                context.ExceptionHandled = true;
            }
            catch (Exception ex)
            {
                var requestUrl = context.HttpContext.Request.Path.ToString();
                LogUtil.Error($"通用异常处理异常：路径{requestUrl}  {ex}");
                // res.Message = "系统繁忙，请稍后再试！";
                res.Message = ex.Message;
                context.Result = new ObjectResult(res);
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
                context.ExceptionHandled = true;
            }
        }
    }
}
