using MediatR;
using Ptcent.Cloud.Drive.Application.Dto.ReponseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ptcent.Cloud.Drive.Application.Dto.RequestModels
{
    /// <summary>
    /// 文件详情入参
    /// </summary>
    public class FileDetailsRequestDto: IRequest<ResponseMessageDto<FileDetailsResponseDto>> //返回值
    {
        /// <summary>
        /// 文件Id
        /// </summary>
        public long FileId { get; set; }
    }
}
