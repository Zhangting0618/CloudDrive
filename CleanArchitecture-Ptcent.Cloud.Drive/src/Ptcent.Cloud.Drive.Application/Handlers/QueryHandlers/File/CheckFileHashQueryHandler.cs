using MediatR;
using Ptcent.Cloud.Drive.Application.Dto.ReponseModels;
using Ptcent.Cloud.Drive.Application.Dto.RequestModels;
using Ptcent.Cloud.Drive.Application.Interfaces.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ptcent.Cloud.Drive.Application.Handlers.QueryHandlers.File
{
    public class CheckFileHashQueryHandler : IRequestHandler<CheckFileHashRequestDto, ResponseMessageDto<bool>>
    {
        private readonly IFileRepository fileRepository;
        public CheckFileHashQueryHandler(IFileRepository fileRepository)
        {
            this.fileRepository = fileRepository;
        }
        public async Task<ResponseMessageDto<bool>> Handle(CheckFileHashRequestDto request, CancellationToken cancellationToken)
        {
          var response=new ResponseMessageDto<bool>() { IsSuccess=true};
            var isFileExit =await fileRepository.Any(a => a.ItemHash == request.FileHash);
            if (isFileExit)
            {
                response.Message = "存在Hash一致的文件";
                response.Data= true;
                return response;
            }
            else
            {
                response.IsSuccess=true;
                response.Data = false;
                return response;
            }
        }
    }
}
