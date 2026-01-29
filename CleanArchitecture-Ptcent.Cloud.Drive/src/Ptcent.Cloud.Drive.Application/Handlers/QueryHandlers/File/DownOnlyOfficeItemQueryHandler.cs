using MediatR;
using Ptcent.Cloud.Drive.Application.Dto.RequestModels;
using Ptcent.Cloud.Drive.Application.Interfaces.Persistence;
using Ptcent.Cloud.Drive.Shared.Util;

namespace Ptcent.Cloud.Drive.Application.Handlers.QueryHandlers.File
{
    public class DownOnlyOfficeItemQueryHandler : IRequestHandler<DownOnlyOfficeItemRequestDto, (string fileType, Stream stream, string archiveName)>
    {
        private readonly IFileRepository fileRepository;
        public DownOnlyOfficeItemQueryHandler(IFileRepository fileRepository)
        {
            this.fileRepository = fileRepository;
        }

        public async Task<(string fileType, Stream stream, string archiveName)> Handle(DownOnlyOfficeItemRequestDto request, CancellationToken cancellationToken)
        {
           //查询文件是否存在
           var fileEntity=await fileRepository.GetFileCacheByItemId(request.FileId);
            if (fileEntity == null)
            {
                throw new  NullReferenceException($"文件{request.FileId}不存在!");
            }
            //判断源文件是否存在
          string  filePath = Path.Combine(ConfigUtil.GetValue("FileRootPath"), fileEntity.PhysicalDirectory);
            if (!System.IO.File.Exists(filePath))
            {
                throw new NullReferenceException($"源文件{fileEntity.PhysicalDirectory}不存在!");
            }
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return ("application/octet-stream", fileStream, fileEntity.LeafName);
        }
    }
}
