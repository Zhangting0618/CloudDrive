using MediatR;
using Microsoft.Extensions.Logging;
using Ptcent.Cloud.Drive.Application.Attributes;
using Ptcent.Cloud.Drive.Application.Contracts.Responses;
using Ptcent.Cloud.Drive.Application.Features.Files.Commands;
using Ptcent.Cloud.Drive.Application.Interfaces;
using Ptcent.Cloud.Drive.Application.Interfaces.Persistence;
using Ptcent.Cloud.Drive.Domain.Enum;
using System.Linq.Expressions;

namespace Ptcent.Cloud.Drive.Application.Handlers.CommandHandlers.File
{
    /// <summary>
    /// 清空回收站命令处理器
    /// </summary>
    [Transactional]
    public class ClearRecycleBinCommandHandler : IRequestHandler<ClearRecycleBinCommand, ResponseMessageDto<bool>>
    {
        private readonly IFileRepository _fileRepository;
        private readonly ILogger<ClearRecycleBinCommandHandler> _logger;

        public ClearRecycleBinCommandHandler(
            IFileRepository fileRepository,
            ILogger<ClearRecycleBinCommandHandler> logger)
        {
            _fileRepository = fileRepository;
            _logger = logger;
        }

        public async Task<ResponseMessageDto<bool>> Handle(ClearRecycleBinCommand request, CancellationToken cancellationToken)
        {
            var response = new ResponseMessageDto<bool> { IsSuccess = true };

            try
            {
                // 获取所有已删除的文件
                var deletedFiles = await _fileRepository.WhereAsync(a =>
                    a.IsDel == (int)FileStatsType.Del);

                var fileList = deletedFiles.ToList();

                // 先处理文件夹（递归删除子项）
                foreach (var folder in fileList.Where(f => f.IsFolder == 1))
                {
                    await DeleteChildrenPermanentlyRecursive(folder.Id);
                }

                // 批量删除
                foreach (var file in fileList)
                {
                    await _fileRepository.DeleteAsync(file, cancellationToken);
                }

                response.Message = "清空回收站成功";
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清空回收站失败：Error={Error}", ex.Message);

                response.IsSuccess = false;
                response.Message = $"清空失败：{ex.Message}";
                return response;
            }
        }

        /// <summary>
        /// 递归彻底删除子文件夹下的所有项
        /// </summary>
        private async Task DeleteChildrenPermanentlyRecursive(long folderId)
        {
            var children = await _fileRepository.WhereAsync(a =>
                a.ParentFolderId == folderId);

            foreach (var child in children)
            {
                // 如果是子文件夹，递归删除
                if (child.IsFolder == 1)
                {
                    await DeleteChildrenPermanentlyRecursive(child.Id);
                }

                // 彻底删除
                await _fileRepository.DeleteAsync(child);
            }
        }
    }
}
