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
    /// 移动文件命令处理器
    /// </summary>
    [Transactional]
    public class MoveFileCommandHandler : IRequestHandler<MoveFileCommand, ResponseMessageDto<bool>>
    {
        private readonly IFileRepository _fileRepository;
        private readonly ILogger<MoveFileCommandHandler> _logger;

        public MoveFileCommandHandler(
            IFileRepository fileRepository,
            ILogger<MoveFileCommandHandler> logger)
        {
            _fileRepository = fileRepository;
            _logger = logger;
        }

        public async Task<ResponseMessageDto<bool>> Handle(MoveFileCommand request, CancellationToken cancellationToken)
        {
            var response = new ResponseMessageDto<bool> { IsSuccess = true };

            try
            {
                // 1. 获取要移动的文件
                var file = await _fileRepository.GetByIdAsync(request.FileId, cancellationToken);
                if (file == null || file.IsDel == (int)FileStatsType.Del)
                {
                    response.IsSuccess = false;
                    response.Message = "文件不存在";
                    return response;
                }

                // 2. 获取目标文件夹
                Ptcent.Cloud.Drive.Domain.Entities.FileEntity? targetFolder = null;
                if (request.NewParentFolderId.HasValue)
                {
                    targetFolder = await _fileRepository.GetByIdAsync(request.NewParentFolderId.Value, cancellationToken);
                    if (targetFolder == null || targetFolder.IsFolder != 1 || targetFolder.IsDel == (int)FileStatsType.Del)
                    {
                        response.IsSuccess = false;
                        response.Message = "目标文件夹不存在";
                        return response;
                    }
                }

                // 3. 检查不能移动到自身或子文件夹
                if (request.NewParentFolderId.HasValue && request.FileId == request.NewParentFolderId.Value)
                {
                    response.IsSuccess = false;
                    response.Message = "不能移动到自身";
                    return response;
                }

                if (file.ParentFolderId == request.NewParentFolderId)
                {
                    response.IsSuccess = false;
                    response.Message = "文件已在目标位置";
                    return response;
                }

                if (file.IsFolder == 1 && targetFolder != null)
                {
                    // 检查目标文件夹是否是当前文件夹的子文件夹
                    if (targetFolder.Idpath.StartsWith(file.Idpath + "/", StringComparison.Ordinal) || targetFolder.Idpath == file.Idpath)
                    {
                        response.IsSuccess = false;
                        response.Message = "不能移动到子文件夹中";
                        return response;
                    }
                }

                // 4. 检查目标文件夹下是否已有同名文件
                var sameNameExists = await _fileRepository.AnyAsync(a =>
                    a.LeafName == file.LeafName &&
                    a.ParentFolderId == request.NewParentFolderId &&
                    a.Id != request.FileId &&
                    a.IsDel == (int)FileStatsType.NoDel);

                if (sameNameExists)
                {
                    response.IsSuccess = false;
                    response.Message = "目标位置已存在同名文件";
                    return response;
                }

                // 5. 更新父级 ID
                file.ParentFolderId = request.NewParentFolderId;

                // 6. 更新路径
                var targetPath = targetFolder?.Path;
                var targetIdPath = targetFolder?.Idpath;
                file.Path = BuildPath(targetPath, file.LeafName);
                file.Idpath = BuildIdPath(targetIdPath, file.Id);

                if (file.IsFolder == 1)
                {
                    await UpdateChildrenPathRecursive(file.Id, file.Path, file.Idpath, cancellationToken);
                }

                file.UpdatedDate = DateTime.Now;
                await _fileRepository.UpdateAsync(file, cancellationToken);

                response.Message = "移动成功";
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "移动文件失败：FileId={FileId}, Error={Error}", request.FileId, ex.Message);

                response.IsSuccess = false;
                response.Message = $"移动失败：{ex.Message}";
                return response;
            }
        }

        /// <summary>
        /// 递归更新子文件夹的路径
        /// </summary>
        private async Task UpdateChildrenPathRecursive(long folderId, string parentPath, string parentIdPath, CancellationToken cancellationToken)
        {
            var children = await _fileRepository.WhereAsync(a =>
                a.ParentFolderId == folderId && a.IsDel == (int)FileStatsType.NoDel);

            foreach (var child in children)
            {
                child.Path = BuildPath(parentPath, child.LeafName);
                child.Idpath = BuildIdPath(parentIdPath, child.Id);
                child.UpdatedDate = DateTime.Now;
                await _fileRepository.UpdateAsync(child, cancellationToken);

                if (child.IsFolder == 1)
                {
                    // 递归更新孙文件夹
                    await UpdateChildrenPathRecursive(child.Id, child.Path, child.Idpath, cancellationToken);
                }
            }
        }

        private static string BuildPath(string? parentPath, string leafName)
        {
            if (string.IsNullOrWhiteSpace(parentPath) || parentPath == "/")
            {
                return "/" + leafName;
            }

            return parentPath.TrimEnd('/') + "/" + leafName;
        }

        private static string BuildIdPath(string? parentIdPath, long id)
        {
            if (string.IsNullOrWhiteSpace(parentIdPath))
            {
                return "/" + id;
            }

            return parentIdPath.TrimEnd('/') + "/" + id;
        }
    }
}
