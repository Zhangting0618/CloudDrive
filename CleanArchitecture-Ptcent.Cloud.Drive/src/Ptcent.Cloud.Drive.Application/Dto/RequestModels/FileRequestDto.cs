using MediatR;
using Ptcent.Cloud.Drive.Application.Dto.ReponseModels;
using Ptcent.Cloud.Drive.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ptcent.Cloud.Drive.Application.Dto.RequestModels
{
    public class FileRequestDto: IRequest<ResponseMessageDto<List<ItemResponseDto>>> //返回值
    {
        /// <summary>
        /// 关键字
        /// </summary>
        public string  KeyWord { get; set; }
        public int  PageIndex { get; set; }
        public int  PageSize { get; set; }
        /// <summary>
        /// 文件类型 
        /// </summary>
        public FileTypeEnum FileType { get; set; }
    }
}
