using MediatR;
using Microsoft.Extensions.Logging;
using Ptcent.Cloud.Drive.Application.Attributes;
using Ptcent.Cloud.Drive.Application.Contracts.Responses;
using Ptcent.Cloud.Drive.Application.Features.Files.Commands;
using Ptcent.Cloud.Drive.Application.Interfaces.Persistence;
using Ptcent.Cloud.Drive.Application.Services;
using Ptcent.Cloud.Drive.Domain.Entities;
using Ptcent.Cloud.Drive.Domain.Enum;

namespace Ptcent.Cloud.Drive.Application.Handlers.CommandHandlers.File
{
    /// <summary>
    /// 复制文件命令处理器
    /// </summary>
    [Transactional]
    public class CopyFileCommandHandler : IRequestHandler<CopyFileCommand, ResponseMessageDto<bool>>
    {
        private readonly IFileRepository _fileRepository;
        private readonly IUserRepository _userRepository;
        private readonly IIdGeneratorService _idGeneratorService;
        private readonly ILogger<CopyFileCommandHandler> _logger;

        public CopyFileCommandHandler(
            IFileRepository fileRepository,
            IUserRepository userRepository,
            IIdGeneratorService idGeneratorService,
            ILogger<CopyFileCommandHandler> logger)
        {
            _fileRepository = fileRepository;
            _userRepository = userRepository;
            _idGeneratorService = idGeneratorService;
            _logger = logger;
        }

        public async Task<ResponseMessageDto<bool>> Handle(CopyFileCommand request, CancellationToken cancellationToken)
        {
            var response = new ResponseMessageDto<bool> { IsSuccess = true, Data = true };

            try
            {
                var source = await _fileRepository.GetByIdAsync(request.FileId, cancellationToken);
                if (source == null || source.IsDel == (int)FileStatsType.Del)
                {
                    response.IsSuccess = false;
                    response.Data = false;
                    response.Message = "源文件不存在";
                    return response;
                }

                FileEntity? targetFolder = null;
                if (request.TargetParentFolderId.HasValue)
                {
                    targetFolder = await _fileRepository.GetByIdAsync(request.TargetParentFolderId.Value, cancellationToken);
                    if (targetFolder == null || targetFolder.IsFolder != 1 || targetFolder.IsDel == (int)FileStatsType.Del)
                    {
                        response.IsSuccess = false;
                        response.Data = false;
                        response.Message = "目标文件夹不存在";
                        return response;
                    }
                }

                if (source.IsFolder == 1 && targetFolder != null)
                {
                    if (targetFolder.Id == source.Id || targetFolder.Idpath.StartsWith(source.Idpath + "/", StringComparison.Ordinal))
                    {
                        response.IsSuccess = false;
                        response.Data = false;
                        response.Message = "不能复制到当前文件夹或其子文件夹中";
                        return response;
                    }
                }

                var rootCopyName = await GenerateCopyNameAsync(source, request.TargetParentFolderId);
                var userId = await _userRepository.UserId();
                var now = DateTime.Now;
                var copiedEntities = new List<FileEntity>();

                await CloneFileRecursiveAsync(
                    source,
                    request.TargetParentFolderId,
                    targetFolder?.Path,
                    targetFolder?.Idpath,
                    rootCopyName,
                    userId,
                    now,
                    copiedEntities,
                    cancellationToken);

                await _fileRepository.AddBatchAsync(copiedEntities, cancellationToken);

                response.Message = "复制成功";
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "复制文件失败：FileId={FileId}, Error={Error}", request.FileId, ex.Message);

                response.IsSuccess = false;
                response.Data = false;
                response.Message = $"复制失败：{ex.Message}";
                return response;
            }
        }

        private async Task CloneFileRecursiveAsync(
            FileEntity source,
            long? targetParentFolderId,
            string? targetParentPath,
            string? targetParentIdPath,
            string targetLeafName,
            long userId,
            DateTime now,
            List<FileEntity> copiedEntities,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var newId = _idGeneratorService.NewId();
            var clone = new FileEntity
            {
                Id = newId,
                LeafName = targetLeafName,
                Extension = source.Extension,
                Path = BuildPath(targetParentPath, targetLeafName),
                ParentFolderId = targetParentFolderId,
                Idpath = BuildIdPath(targetParentIdPath, newId),
                IsFolder = source.IsFolder,
                FileType = source.FileType,
                IsDel = (int)FileStatsType.NoDel,
                CreatedDate = now,
                UpdatedDate = now,
                CreatedBy = userId,
                UpdatedBy = userId,
                DeletedDate = null,
                DeletedBy = null,
                VersionId = _idGeneratorService.NewId(),
                ItemHash = source.ItemHash,
                FileSize = source.FileSize,
                PhysicalDirectory = source.PhysicalDirectory,
                ItemFileMapUrl = source.IsFolder == 1 ? null : $"/api/file/{newId}/preview"
            };

            copiedEntities.Add(clone);

            if (source.IsFolder != 1)
            {
                return;
            }

            var children = (await _fileRepository.WhereAsync(a =>
                    a.ParentFolderId == source.Id &&
                    a.IsDel == (int)FileStatsType.NoDel))
                .OrderByDescending(a => a.IsFolder)
                .ThenBy(a => a.LeafName)
                .ToList();

            foreach (var child in children)
            {
                await CloneFileRecursiveAsync(
                    child,
                    clone.Id,
                    clone.Path,
                    clone.Idpath,
                    child.LeafName,
                    userId,
                    now,
                    copiedEntities,
                    cancellationToken);
            }
        }

        private async Task<string> GenerateCopyNameAsync(FileEntity source, long? targetParentFolderId)
        {
            var siblingNames = (await _fileRepository.WhereAsync(a =>
                    a.ParentFolderId == targetParentFolderId &&
                    a.IsDel == (int)FileStatsType.NoDel))
                .Select(a => a.LeafName)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var index = 1;
            while (true)
            {
                var candidate = BuildCopyLeafName(source.LeafName, source.Extension, index);
                if (!siblingNames.Contains(candidate))
                {
                    return candidate;
                }

                index++;
            }
        }

        private static string BuildCopyLeafName(string leafName, string? extension, int index)
        {
            if (string.IsNullOrWhiteSpace(extension) ||
                !leafName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
            {
                return index == 1
                    ? $"{leafName} - 副本"
                    : $"{leafName} - 副本 ({index})";
            }

            var baseName = leafName[..^extension.Length];
            return index == 1
                ? $"{baseName} - 副本{extension}"
                : $"{baseName} - 副本 ({index}){extension}";
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
