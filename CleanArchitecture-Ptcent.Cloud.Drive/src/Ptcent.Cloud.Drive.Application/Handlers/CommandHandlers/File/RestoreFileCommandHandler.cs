using MediatR;
using Microsoft.Extensions.Logging;
using Ptcent.Cloud.Drive.Application.Attributes;
using Ptcent.Cloud.Drive.Application.Contracts.Responses;
using Ptcent.Cloud.Drive.Application.Features.Files.Commands;
using Ptcent.Cloud.Drive.Application.Interfaces;
using Ptcent.Cloud.Drive.Application.Interfaces.Persistence;
using Ptcent.Cloud.Drive.Domain.Enum;

namespace Ptcent.Cloud.Drive.Application.Handlers.CommandHandlers.File
{
    /// <summary>
    /// 还原文件命令处理器
    /// </summary>
    [Transactional]
    public class RestoreFileCommandHandler : IRequestHandler<RestoreFileCommand, ResponseMessageDto<bool>>
    {
        private readonly IFileRepository _fileRepository;
        private readonly ILogger<RestoreFileCommandHandler> _logger;

        public RestoreFileCommandHandler(
            IFileRepository fileRepository,
            ILogger<RestoreFileCommandHandler> logger)
        {
            _fileRepository = fileRepository;
            _logger = logger;
        }

        public async Task<ResponseMessageDto<bool>> Handle(RestoreFileCommand request, CancellationToken cancellationToken)
        {
            var response = new ResponseMessageDto<bool> { IsSuccess = true };

            try
            {
                // 1. 获取已删除的文件
                var file = await _fileRepository.GetByIdAsync(request.FileId, cancellationToken);
                if (file == null || file.IsDel != (int)FileStatsType.Del)
                {
                    response.IsSuccess = false;
                    response.Message = "文件不存在或不在回收站中";
                    return response;
                }

                // 2. 检查原父文件夹是否存在
                if (file.ParentFolderId.HasValue)
                {
                    var parentFolder = await _fileRepository.GetByIdAsync(file.ParentFolderId.Value);
                    if (parentFolder == null || parentFolder.IsDel == (int)FileStatsType.Del)
                    {
                        // 原父文件夹不存在，恢复到根目录
                        file.ParentFolderId = null;
                    }
                }

                // 3. 检查是否有同名文件
                var sameNameExists = await _fileRepository.AnyAsync(a =>
                    a.LeafName == file.LeafName &&
                    a.ParentFolderId == file.ParentFolderId &&
                    a.Id != request.FileId &&
                    a.IsDel == (int)FileStatsType.NoDel);

                if (sameNameExists)
                {
                    response.IsSuccess = false;
                    response.Message = "原位置已存在同名文件，请在文件管理中手动处理";
                    return response;
                }

                // 4. 还原文件
                file.IsDel = (int)FileStatsType.NoDel;
                file.DeletedDate = null;
                file.DeletedBy = null;
                file.UpdatedDate = DateTime.Now;

                // 如果是文件夹，递归还原子项
                if (file.IsFolder == 1)
                {
                    await RestoreChildrenRecursive(file.Id);
                }

                await _fileRepository.UpdateAsync(file, cancellationToken);

                response.Message = "还原成功";
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "还原文件失败：FileId={FileId}, Error={Error}", request.FileId, ex.Message);

                response.IsSuccess = false;
                response.Message = $"还原失败：{ex.Message}";
                return response;
            }
        }

        /// <summary>
        /// 递归还原子文件夹下的所有项
        /// </summary>
        private async Task RestoreChildrenRecursive(long folderId)
        {
            var children = await _fileRepository.WhereAsync(a =>
                a.ParentFolderId == folderId && a.IsDel == (int)FileStatsType.Del);

            foreach (var child in children)
            {
                child.IsDel = (int)FileStatsType.NoDel;
                child.DeletedDate = null;
                child.DeletedBy = null;

                // 如果是子文件夹，递归还原
                if (child.IsFolder == 1)
                {
                    await RestoreChildrenRecursive(child.Id);
                }

                await _fileRepository.UpdateAsync(child);
            }
        }
    }
}
