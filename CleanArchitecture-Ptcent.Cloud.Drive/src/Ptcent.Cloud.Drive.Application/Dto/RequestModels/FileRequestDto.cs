using MediatR;
using Ptcent.Cloud.Drive.Application.Contracts.Responses;
using Ptcent.Cloud.Drive.Application.Dto.ReponseModels;
using Ptcent.Cloud.Drive.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ptcent.Cloud.Drive.Application.Dto.RequestModels
{
    public class FileRequestDto : IRequest<ResponseMessageDto<List<ItemResponseDto>>>
    {
        /// <summary>
        /// 父文件夹 ID
        /// </summary>
        public long? ParentFolderId { get; set; }

        /// <summary>
        /// 关键字
        /// </summary>
        public string KeyWord { get; set; } = string.Empty;

        /// <summary>
        /// 页码
        /// </summary>
        public int PageIndex { get; set; } = 1;

        /// <summary>
        /// 每页数量
        /// </summary>
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// 文件类型
        /// </summary>
        public FileTypeEnum FileType { get; set; } = FileTypeEnum.All;
    }
}
