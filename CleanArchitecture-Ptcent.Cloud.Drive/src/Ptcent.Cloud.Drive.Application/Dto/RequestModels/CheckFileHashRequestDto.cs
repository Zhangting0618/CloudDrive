using MediatR;
using Ptcent.Cloud.Drive.Application.Dto.ReponseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ptcent.Cloud.Drive.Application.Dto.RequestModels
{
    public class CheckFileHashRequestDto:IRequest<ResponseMessageDto<bool>>
    {
        /// <summary>
        /// 文件hash
        /// </summary>
        public string  FileHash { get; set; }
    }
}
