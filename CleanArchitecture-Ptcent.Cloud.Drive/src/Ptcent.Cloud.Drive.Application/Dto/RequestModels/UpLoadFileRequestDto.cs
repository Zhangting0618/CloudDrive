using MediatR;
using Microsoft.AspNetCore.Http;
using Ptcent.Cloud.Drive.Application.Dto.ReponseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ptcent.Cloud.Drive.Application.Dto.RequestModels
{
    /// <summary>
    /// 上传文件信息
    /// </summary>
    public class UpLoadFileRequestDto:IRequest<ResponseMessageDto<bool>>
    {
        /// <summary>
        /// 上传文件
        /// </summary>
        public IFormFileCollection FormFiles {  get; set; }
        /// <summary>
        /// 当前上传文件第几片
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        ///文件总大小
        /// </summary>
        public long Size { get; set; }
        /// <summary>
        /// 文件总片数
        /// </summary>
        public string Total { get; set; }
    }
}
