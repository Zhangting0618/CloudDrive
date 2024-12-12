using MediatR;
using Ptcent.Cloud.Drive.Application.Dto.ReponseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ptcent.Cloud.Drive.Application.Dto.RequestModels
{
    public class MergeChunkFileRequestDto:IRequest<ResponseMessageDto<bool>>
    {
        /// <summary>
        /// 文件Hash
        /// </summary>
        public string FileHash { get; set; }
        /// <summary>
        /// 文件名称
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 父文件Id
        /// </summary>
        public long? ParentFloderId { get; set; }
        /// <summary>
        /// 文件大小
        /// </summary>
        public long FileSize { get; set; }
    }
}
