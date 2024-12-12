using MediatR;
using Ptcent.Cloud.Drive.Application.Dto.Common;
using Ptcent.Cloud.Drive.Application.Dto.ReponseModels;
using Ptcent.Cloud.Drive.Application.Dto.RequestModels;
using Ptcent.Cloud.Drive.Domain.Entities;
using Ptcent.Cloud.Drive.Domain.Enum;
using Ptcent.Cloud.Drive.Infrastructure.IRespository;
using Yitter.IdGenerator;

namespace Ptcent.Cloud.Drive.Application.Handlers.CommandHandlers.File
{
    public class CreateFolderCommandHandler : IRequestHandler<CreateFolderRequestDto, ResponseMessageDto<bool>>
    {
        private readonly IFileRepository fileRepository;
        private readonly IUserRepository userRepository;
        private readonly IIdGenerator idGenerator;
        public CreateFolderCommandHandler(IFileRepository fileRepository,IIdGenerator idGenerator, IUserRepository userRepository)
        {
            this.fileRepository = fileRepository;
            this.idGenerator = idGenerator;
            this.userRepository = userRepository;
        }
        public async Task<ResponseMessageDto<bool>> Handle(CreateFolderRequestDto request, CancellationToken cancellationToken)
        {
            var response = new ResponseMessageDto<bool>() { IsSuccess=true};
            long? folderId = request.ParentFolderId == null ? null : request.ParentFolderId;
            //判断要创建的文件夹名称是否存在
            var folderExit = await fileRepository.Any(a => a.LeafName == request.FolderName && a.ParentFolderId == folderId && a.IsDel == (int)FileStatsType.NoDel);
            if (folderExit)
            {
                response.IsSuccess = false;
                response.Message = $"存在相同名称{request.FolderName}的文件夹!";
                return response;
            }
            FileEntity fileEntity = new FileEntity();
            fileEntity.Id = idGenerator.NewLong();
            fileEntity.ParentFolderId = folderId;
            fileEntity.VersionId = idGenerator.NewLong();
            fileEntity.LeafName = request.FolderName;
            fileEntity.FileSize = 0;
            fileEntity.IsFolder = 1;
            fileEntity.Extension = string.Empty;
            fileEntity.FileType = (int)FileTypeEnum.Folder;
            fileEntity.IsDel = (int)FileStatsType.NoDel;
            if (folderId == null)
            {
                //代表在最顶成创建
                fileEntity.Path = $"/{fileEntity.LeafName}";
                fileEntity.Idpath = $"/{fileEntity.Idpath}";
            }
            else
            {
                var parentFileEntity = (await fileRepository.WhereAsync(a => a.ParentFolderId == request.ParentFolderId)).Select(a => new
                {
                    a.Path,
                    a.Idpath
                }).FirstOrDefault();
                fileEntity.Path = parentFileEntity?.Path + $"/{fileEntity.LeafName}";
                fileEntity.Idpath = parentFileEntity?.Idpath + $"/{fileEntity.Idpath}";
            }
            await fileRepository.SaveFileEntity(fileEntity,await userRepository.UserId(), true);
            response.Message = "创建成功";
            return response;
        }
    }
}
