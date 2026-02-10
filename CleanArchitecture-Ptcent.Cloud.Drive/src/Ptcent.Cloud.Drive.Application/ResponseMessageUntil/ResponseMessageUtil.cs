using Ptcent.Cloud.Drive.Application.Dto.ReponseModels;
using Ptcent.Cloud.Drive.Domain.Enum;
using Ptcent.Cloud.Drive.Shared.Util;
using Ptcent.Cloud.Drive.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ptcent.Cloud.Drive.Application.ResponseMessageUntil
{
    public sealed class ResponseMessageUtil : IResponseMessageUtil
    {
        private readonly IRequestContext _requestContext;

        public ResponseMessageUtil(IRequestContext requestContext)
        {
            _requestContext = requestContext;
        }

        public ResponseMessageDto<T> CaptureException<T>(Exception ex, string exceptionMsg)
        {
            var res = new ResponseMessageDto<T>
            {
                Data = default,
                TotalCount = 0,
                Code = WebApiResultCode.SystemError,
                IsSuccess = false,
                Message = "系统异常,请稍后再试"
            };

            try
            {
                var context = _requestContext.Current;
                if (context != null)
                {
                    var authToken = context.Request.Headers["AuthToken"].ToString();

                    LogUtil.Error(
                        $"请求异常：{exceptionMsg}\r\n{ex}\r\n{ex.InnerException?.Message}\r\n" +
                        $"请求参数：{GetHttpRequestDataStr()}\r\n用户信息：{authToken}");
                }
            }
            catch (Exception logEx)
            {
                LogUtil.Error($"CaptureException异常：{logEx}");
            }

            return res;
        }

        public string GetHttpRequestDataStr()
        {
            var context = _requestContext.Current;
            if (context == null) return string.Empty;

            var request = context.Request;
            var method = request.Method.ToLowerInvariant();

            if (method == "post")
            {
                if (request.Body.CanSeek)
                {
                    request.Body.Position = 0;
                    using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
                    var body = reader.ReadToEnd();
                    request.Body.Position = 0;
                    return body;
                }
            }
            else if (method == "get")
            {
                if (request.Query.Any())
                {
                    return request.Query
                        .OrderBy(q => q.Key)
                        .ToDictionary(q => q.Key, q => q.Value.ToString())
                        .ToJson();
                }
            }

            return string.Empty;
        }
    }

}
