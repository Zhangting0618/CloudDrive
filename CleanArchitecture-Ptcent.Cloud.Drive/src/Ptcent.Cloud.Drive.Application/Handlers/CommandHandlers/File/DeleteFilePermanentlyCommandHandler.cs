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
    /// 彻底删除文件命令处理器
    /// </summary>
    [Transactional]
    public class DeleteFilePermanentlyCommandHandler : IRequestHandler<DeleteFilePermanentlyCommand, ResponseMessageDto<bool>>
    {
        private readonly IFileRepository _fileRepository;
        private readonly ILogger<DeleteFilePermanentlyCommandHandler> _logger;

        public DeleteFilePermanentlyCommandHandler(
            IFileRepository fileRepository,
            ILogger<DeleteFilePermanentlyCommandHandler> logger)
        {
            _fileRepository = fileRepository;
            _logger = logger;
        }

        public async Task<ResponseMessageDto<bool>> Handle(DeleteFilePermanentlyCommand request, CancellationToken cancellationToken)
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

                // 2. 如果是文件夹，递归删除子项
                if (file.IsFolder == 1)
                {
                    await DeleteChildrenPermanentlyRecursive(file.Id);
                }

                // 3. 彻底删除文件（从数据库删除）
                await _fileRepository.DeleteAsync(file, cancellationToken);

                response.Message = "彻底删除成功";
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "彻底删除文件失败：FileId={FileId}, Error={Error}", request.FileId, ex.Message);

                response.IsSuccess = false;
                response.Message = $"彻底删除失败：{ex.Message}";
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
