using MediatR;
using Microsoft.Extensions.Logging;
using Ptcent.Cloud.Drive.Application.Attributes;
using Ptcent.Cloud.Drive.Application.Contracts.Responses;
using Ptcent.Cloud.Drive.Application.Dto.ReponseModels;
using Ptcent.Cloud.Drive.Application.Interfaces;
using Ptcent.Cloud.Drive.Application.Interfaces.Persistence;
using Ptcent.Cloud.Drive.Domain.Entities;
using Ptcent.Cloud.Drive.Domain.Enum;

namespace Ptcent.Cloud.Drive.Application.Handlers.CommandHandlers.File
{
    /// <summary>
    /// 文件转移命令处理器
    /// 演示事务管理和独立提交的使用
    /// </summary>
    /// <remarks>
    /// 这是一个示例 Handler，需要根据实际项目结构调整
    /// </remarks>
    [Transactional(IsolationLevel = Application.Attributes.IsolationLevel.ReadCommitted, TimeoutSeconds = 30)]
    public class TransferFileCommandHandler : IRequestHandler<TransferFileRequestDto, ResponseMessageDto<bool>>
    {
        private readonly IFileRepository _fileRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TransferFileCommandHandler> _logger;

        public TransferFileCommandHandler(
            IFileRepository fileRepository,
            IUnitOfWork unitOfWork,
            ILogger<TransferFileCommandHandler> logger)
        {
            _fileRepository = fileRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ResponseMessageDto<bool>> Handle(TransferFileRequestDto request, CancellationToken cancellationToken)
        {
            var response = new ResponseMessageDto<bool> { IsSuccess = true };

            try
            {
                // 1. 获取源文件
                var sourceFile = await _fileRepository.GetByIdAsync(request.FileId, cancellationToken);
                if (sourceFile == null)
                {
                    response.IsSuccess = false;
                    response.Message = "文件不存在";
                    return response;
                }

                // 2. 检查目标文件夹是否存在
                if (request.TargetFolderId.HasValue)
                {
                    var targetFolder = await _fileRepository.GetByIdAsync(request.TargetFolderId.Value, cancellationToken);
                    if (targetFolder == null || targetFolder.IsFolder != 1)
                    {
                        response.IsSuccess = false;
                        response.Message = "目标文件夹不存在";
                        return response;
                    }
                }

                // 3. 更新文件路径（在事务中）
                sourceFile.ParentFolderId = request.TargetFolderId;
                await _fileRepository.UpdateAsync(sourceFile, cancellationToken);

                // 4. 保存更改（由 TransactionBehavior 自动提交）
                // 如果需要手动控制，可以使用 _unitOfWork

                response.Message = "文件转移成功";
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "文件转移失败：FileId={FileId}, Error={Error}", request.FileId, ex.Message);

                response.IsSuccess = false;
                response.Message = $"文件转移失败：{ex.Message}";
                return response;
            }
        }
    }

    /// <summary>
    /// 转移文件请求 DTO
    /// </summary>
    public class TransferFileRequestDto : IRequest<ResponseMessageDto<bool>>
    {
        public long FileId { get; set; }
        public long? TargetFolderId { get; set; }
        public long OriginalFolderId { get; set; }
        public long UserId { get; set; }
    }
}
