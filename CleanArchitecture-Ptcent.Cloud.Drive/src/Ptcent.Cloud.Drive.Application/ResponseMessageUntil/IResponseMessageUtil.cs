using Ptcent.Cloud.Drive.Application.Dto.ReponseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ptcent.Cloud.Drive.Application.ResponseMessageUntil
{
    public interface IResponseMessageUtil
    {
        ResponseMessageDto<T> CaptureException<T>(Exception ex, string exceptionMsg);
        string GetHttpRequestDataStr();
    }

}
