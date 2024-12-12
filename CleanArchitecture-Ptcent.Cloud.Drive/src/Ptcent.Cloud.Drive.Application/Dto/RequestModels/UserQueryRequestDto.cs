using MediatR;
using Ptcent.Cloud.Drive.Application.Dto.ReponseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ptcent.Cloud.Drive.Application.Dto.RequestModels
{
    public class UserQueryRequestDto :IRequest<ResponseMessageDto<bool>>
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public long UserId { get; set; }
    }
}
