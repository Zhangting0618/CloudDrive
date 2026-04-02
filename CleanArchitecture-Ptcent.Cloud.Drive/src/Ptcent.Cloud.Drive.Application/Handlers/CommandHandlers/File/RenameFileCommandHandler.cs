using MediatR;
using Microsoft.Extensions.Logging;
using Ptcent.Cloud.Drive.Application.Attributes;
using Ptcent.Cloud.Drive.Application.Contracts.Responses;
using Ptcent.Cloud.Drive.Application.Features.Files.Commands;
using Ptcent.Cloud.Drive.Application.Interfaces.Persistence;
using Ptcent.Cloud.Drive.Domain.Enum;

namespace Ptcent.Cloud.Drive.Application.Handlers.CommandHandlers.File
{
    /// <summary>
    /// 重命名文件命令处理器
    /// </summary>
    [Transactional]
    public class RenameFileCommandHandler : IRequestHandler<RenameFileCommand, ResponseMessageDto<bool>>
    {
        private readonly IFileRepository _fileRepository;
        private readonly ILogger<RenameFileCommandHandler> _logger;

        public RenameFileCommandHandler(
            IFileRepository fileRepository,
            ILogger<RenameFileCommandHandler> logger)
        {
            _fileRepository = fileRepository;
            _logger = logger;
        }

        public async Task<ResponseMessageDto<bool>> Handle(RenameFileCommand request, CancellationToken cancellationToken)
        {
            var response = new ResponseMessageDto<bool> { IsSuccess = true };

            try
            {
                // 1. 获取文件
                var file = await _fileRepository.GetByIdAsync(request.FileId, cancellationToken);
                if (file == null || file.IsDel == (int)FileStatsType.Del)
                {
                    response.IsSuccess = false;
                    response.Message = "文件不存在";
                    return response;
                }

                // 2. 验证新名称
                if (string.IsNullOrWhiteSpace(request.NewName))
                {
                    response.IsSuccess = false;
                    response.Message = "文件名不能为空";
                    return response;
                }

                // 3. 检查同名文件是否存在
                var newName = request.NewName.Trim();
                var sameNameExists = await _fileRepository.AnyAsync(a =>
                    a.LeafName == newName &&
                    a.ParentFolderId == file.ParentFolderId &&
                    a.Id != request.FileId &&
                    a.IsDel == (int)FileStatsType.NoDel);

                if (sameNameExists)
                {
                    response.IsSuccess = false;
                    response.Message = "该名称已存在";
                    return response;
                }

                // 4. 更新文件名和路径
                string oldLeafName = file.LeafName;
                file.LeafName = newName;

                // 更新路径（如果是文件夹，需要同步更新 Idpath）
                if (file.IsFolder == 1)
                {
                    // 更新文件夹自身的路径
                    var parentFolder = await _fileRepository.GetByIdAsync(file.ParentFolderId);
                    if (parentFolder != null)
                    {
                        file.Path = parentFolder.Path + "/" + newName;
                    }
                    else
                    {
                        file.Path = "/" + newName;
                    }

                    // 更新所有子文件夹的路径前缀
                    await UpdateChildrenPathRecursive(file.Id, file.Path, file.Idpath);
                }
                else
                {
                    // 文件：只更新文件名部分的路径
                    var pathParts = file.Path?.Split('/').ToList() ?? new List<string>();
                    if (pathParts.Count > 0)
                    {
                        pathParts[pathParts.Count - 1] = newName;
                        file.Path = string.Join("/", pathParts);
                    }
                }

                file.UpdatedDate = DateTime.Now;
                await _fileRepository.UpdateAsync(file, cancellationToken);

                response.Message = "重命名成功";
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "重命名文件失败：FileId={FileId}, Error={Error}", request.FileId, ex.Message);

                response.IsSuccess = false;
                response.Message = $"重命名失败：{ex.Message}";
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
