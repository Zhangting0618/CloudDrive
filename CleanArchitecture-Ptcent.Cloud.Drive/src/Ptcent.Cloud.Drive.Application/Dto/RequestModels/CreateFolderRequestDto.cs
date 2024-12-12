using MediatR;
using Ptcent.Cloud.Drive.Application.Dto.ReponseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ptcent.Cloud.Drive.Application.Dto.RequestModels
{
    public class CreateFolderRequestDto:IRequest<ResponseMessageDto<bool>> //返回值
    {
        /// <summary>
        /// 文件夹名称
        /// </summary>
        public string FolderName { get; set; }
        /// <summary>
        /// 上级文件夹Id 最顶层 传空
        /// </summary>
        public long? ParentFolderId { get; set; }
    }
}
