using MediatR;
using Microsoft.Extensions.Logging;
using Ptcent.Cloud.Drive.Application.Attributes;
using Ptcent.Cloud.Drive.Application.Contracts.Responses;
using Ptcent.Cloud.Drive.Application.Features.Files.Commands;
using Ptcent.Cloud.Drive.Application.Interfaces;
using Ptcent.Cloud.Drive.Application.Interfaces.Persistence;
using Ptcent.Cloud.Drive.Domain.Entities;
using Ptcent.Cloud.Drive.Domain.Enum;

namespace Ptcent.Cloud.Drive.Application.Handlers.CommandHandlers.File
{
    /// <summary>
    /// 删除文件命令处理器（软删除）
    /// </summary>
    [Transactional]
    public class DeleteFileCommandHandler : IRequestHandler<DeleteFileCommand, ResponseMessageDto<bool>>
    {
        private readonly IFileRepository _fileRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<DeleteFileCommandHandler> _logger;

        public DeleteFileCommandHandler(
            IFileRepository fileRepository,
            IUserRepository userRepository,
            ILogger<DeleteFileCommandHandler> logger)
        {
            _fileRepository = fileRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<ResponseMessageDto<bool>> Handle(DeleteFileCommand request, CancellationToken cancellationToken)
        {
            var response = new ResponseMessageDto<bool> { IsSuccess = true };

            try
            {
                // 1. 获取文件
                var file = await _fileRepository.GetByIdAsync(request.FileId, cancellationToken);
                if (file == null || file.IsDel == (int)FileStatsType.Del)
                {
                    response.IsSuccess = false;
                    response.Message = "文件不存在或已被删除";
                    return response;
                }

                // 2. 执行软删除
                file.IsDel = (int)FileStatsType.Del;
                file.DeletedDate = DateTime.Now;
                file.DeletedBy = await _userRepository.UserId();

                // 如果是文件夹，递归删除子文件夹和文件
                if (file.IsFolder == 1)
                {
                    await DeleteFolderRecursive(file.Id, file.Idpath);
                }

                await _fileRepository.UpdateAsync(file, cancellationToken);

                response.Message = "删除成功";
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除文件失败：FileId={FileId}, Error={Error}", request.FileId, ex.Message);

                response.IsSuccess = false;
                response.Message = $"删除失败：{ex.Message}";
                return response;
            }
        }

        /// <summary>
        /// 递归删除文件夹下的所有子项
        /// </summary>
        private async Task DeleteFolderRecursive(long folderId, string idPath)
        {
            var children = await _fileRepository.WhereAsync(a =>
                a.ParentFolderId == folderId && a.IsDel == (int)FileStatsType.NoDel);

            foreach (var child in children)
            {
                child.IsDel = (int)FileStatsType.Del;
                child.DeletedDate = DateTime.Now;
                child.DeletedBy = await _userRepository.UserId();

                // 如果是子文件夹，递归删除
                if (child.IsFolder == 1)
                {
                    await DeleteFolderRecursive(child.Id, child.Idpath);
                }

                await _fileRepository.UpdateAsync(child);
            }
        }
    }
}
