using MediatR;
using Microsoft.Extensions.Configuration;
using Ptcent.Cloud.Drive.Application.Dto.ReponseModels;
using Ptcent.Cloud.Drive.Application.Dto.RequestModels;
using Ptcent.Cloud.Drive.Domain.Enum;
using Ptcent.Cloud.Drive.Infrastructure.IRespository;
using Ptcent.Cloud.Drive.Shared.Extensions;

namespace Ptcent.Cloud.Drive.Application.Handlers.QueryHandlers.File
{
    /// <summary>
    /// 文件详情
    /// </summary>
    public class GetFileDetailQueryHandler : IRequestHandler<FileDetailsRequestDto, ResponseMessageDto<FileDetailsResponseDto>>
    {
        private readonly IFileRepository fileRepository;
        private readonly IConfiguration config;
        public GetFileDetailQueryHandler(IFileRepository fileRepository, IConfiguration config)
        {
            this.fileRepository = fileRepository;
            this.config = config;
        }
        public async Task<ResponseMessageDto<FileDetailsResponseDto>> Handle(FileDetailsRequestDto request, CancellationToken cancellationToken)
        {
            var response=new ResponseMessageDto<FileDetailsResponseDto>() { IsSuccess=true};
            //查询文件是否存在
            var fileEntity=await fileRepository.GetById(request.FileId);
            if (fileEntity == null)
            {
                response.IsSuccess = false;
                response.Data = null;
                response.Message = $"文件{request.FileId}不存在";
                return response;
            }
            //存在则查询文件是否存在
            var filePath = Path.Combine(config["FileRootPath"], fileEntity.ItemFileMapUrl.IsNullOrWhiteSpace() ? fileEntity.PhysicalDirectory : fileEntity.ItemFileMapUrl);
            if (!System.IO.File.Exists(filePath))
            {
                response.IsSuccess = false;
                response.Data = null;
                response.Message = $"源文件{fileEntity.LeafName}不存在";
                return response;
            }
            //获取文件类型 生成不同的缩略图
            if (fileEntity.FileType== (int)FileTypeEnum.Docs)
            {
                //代表文档 里面细分office三件套
            }
          else  if (fileEntity.FileType == (int)FileTypeEnum.Image)
            {

            }
          else if (fileEntity.FileType == (int)FileTypeEnum.Video)
            {

            }
            else
            {
                //代表其他
            }
            return response;
        }
    }
}
