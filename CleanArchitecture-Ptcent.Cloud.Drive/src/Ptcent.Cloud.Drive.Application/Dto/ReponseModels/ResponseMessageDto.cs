using Ptcent.Cloud.Drive.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ptcent.Cloud.Drive.Application.Dto.ReponseModels
{
    /// <summary>
    /// 返回结果基类
    /// </summary>
    public class ResponseMessageDto<T>
    {
        /// <summary>
        /// 消息提示
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; }
        /// <summary>
        /// 消息状态码
        /// </summary>
        public WebApiResultCode Code { get; set; }
        /// <summary>
        /// 总条数
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 返回数据
        /// </summary>
        public T Data { get; set; }
    }
}
