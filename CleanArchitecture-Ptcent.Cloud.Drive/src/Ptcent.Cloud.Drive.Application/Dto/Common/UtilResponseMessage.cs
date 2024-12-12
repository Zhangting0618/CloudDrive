using Microsoft.Extensions.Primitives;
using Ptcent.Cloud.Drive.Application.Dto.ReponseModels;
using Ptcent.Cloud.Drive.Domain.Enum;
using Ptcent.Cloud.Drive.Shared.Extensions;
using Ptcent.Cloud.Drive.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ptcent.Cloud.Drive.Application.Dto.Common
{
    public class UtilResponseMessage
    {
        public static ResponseMessageDto<T> CaptureException<T>(Exception ex, string exceptionMsg)
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
                var authToken = Convert.ToString(HttpContextUtil.Current.Request.Headers["AuthToken"]);
                LogUtil.Error($"请求异常：{exceptionMsg}\r\n{ex}\r\n {ex.InnerException?.Message}\r\n请求参数：{GetHttpRequestDataStr()}\r\n用户信息：{authToken}");
            }
            catch (Exception ex1)
            {
                LogUtil.Error($"CaptureException异常：{ex1}");
            }
            return res;
        }

        /// <summary>
        /// 获取请求参数信息
        /// </summary>
        public static string GetHttpRequestDataStr()
        {
            var requestDataStr = string.Empty;
            if (HttpContextUtil.Current.Request.Method.Trim().ToLower() == "post")
            {
                Stream stream = HttpContextUtil.Current.Request.Body;
                if (stream != null)
                {
                    Encoding encoding = Encoding.UTF8;
                    stream.Position = 0;
                    using (var reader = new StreamReader(stream, encoding))
                    {
                        requestDataStr = reader.ReadToEndAsync().Result.ToEntity<object>().ToJson();
                        requestDataStr = requestDataStr == "null" ? "" : requestDataStr;
                    }
                }
            }
            else if (HttpContextUtil.Current.Request.Method.Trim().ToLower() == "get")
            {
                if (HttpContextUtil.Current.Request.Query.Count > 0)
                {
                    var parameters = new Dictionary<string, string>();
                    var query = HttpContextUtil.Current.Request.Query;
                    for (int f = 0; f < query.Count; f++)
                    {
                        var keys = query.Keys.ToList();
                        query.TryGetValue(keys[f], out StringValues value);
                        parameters.Add(keys[f], value.ToString());
                    }
                    var sortedParams = new SortedDictionary<string, string>(parameters);
                    requestDataStr = sortedParams.ToJson();
                }
            }
            return requestDataStr;
        }
    }
}
