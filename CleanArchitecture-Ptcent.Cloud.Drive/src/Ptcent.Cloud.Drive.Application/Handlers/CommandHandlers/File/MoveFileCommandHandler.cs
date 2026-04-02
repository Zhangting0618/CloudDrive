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
                var targetFolder = await _fileRepository.GetByIdAsync(request.NewParentFolderId, cancellationToken);
                if (targetFolder == null || targetFolder.IsFolder != 1 || targetFolder.IsDel == (int)FileStatsType.Del)
                {
                    response.IsSuccess = false;
                    response.Message = "目标文件夹不存在";
                    return response;
                }

                // 3. 检查不能移动到自身或子文件夹
                if (request.FileId == request.NewParentFolderId)
                {
                    response.IsSuccess = false;
                    response.Message = "不能移动到自身";
                    return response;
                }

                if (file.IsFolder == 1)
                {
                    // 检查目标文件夹是否是当前文件夹的子文件夹
                    if (targetFolder.Idpath.StartsWith(file.Idpath + "/") || targetFolder.Idpath == file.Idpath)
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
                long? oldParentId = file.ParentFolderId;
                file.ParentFolderId = request.NewParentFolderId;

                // 6. 更新路径
                if (file.IsFolder == 1)
                {
                    // 文件夹：更新自身路径和所有子项路径
                    file.Path = targetFolder.Path + "/" + file.LeafName;
                    file.Idpath = targetFolder.Idpath + "/" + file.Id;

                    await UpdateChildrenPathRecursive(file.Id, file.Path, file.Idpath);
                }
                else
                {
                    // 文件：只更新路径中的父级部分
                    var pathParts = file.Path?.Split('/').ToList() ?? new List<string>();
                    if (pathParts.Count > 0)
                    {
                        pathParts[pathParts.Count - 1] = file.LeafName;
                        file.Path = targetFolder.Path + "/" + file.LeafName;
                    }
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
        private async Task UpdateChildrenPathRecursive(long folderId, string parentPath, string parentIdPath)
        {
            var children = await _fileRepository.WhereAsync(a =>
                a.ParentFolderId == folderId && a.IsDel == (int)FileStatsType.NoDel);

            foreach (var child in children)
            {
                if (child.IsFolder == 1)
                {
                    // 更新子文件夹路径
                    child.Path = parentPath + "/" + child.LeafName;
                    child.Idpath = parentIdPath + "/" + child.Id;
                    await _fileRepository.UpdateAsync(child);

                    // 递归更新孙文件夹
                    await UpdateChildrenPathRecursive(child.Id, child.Path, child.Idpath);
                }
            }
        }
    }
}
