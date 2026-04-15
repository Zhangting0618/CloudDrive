using MediatR;
using Microsoft.Extensions.Logging;
using Ptcent.Cloud.Drive.Application.Attributes;
using Ptcent.Cloud.Drive.Application.Contracts.Responses;
using Ptcent.Cloud.Drive.Application.Features.Files.Commands;
using Ptcent.Cloud.Drive.Application.Interfaces;

namespace Ptcent.Cloud.Drive.Application.Handlers.CommandHandlers.File
{
    /// <summary>
    /// 清空回收站命令处理器
    /// </summary>
    [Transactional]
    public class ClearRecycleBinCommandHandler : IRequestHandler<ClearRecycleBinCommand, ResponseMessageDto<bool>>
    {
        private readonly IRecycleBinCleanupService _recycleBinCleanupService;
        private readonly ILogger<ClearRecycleBinCommandHandler> _logger;

        public ClearRecycleBinCommandHandler(
            IRecycleBinCleanupService recycleBinCleanupService,
            ILogger<ClearRecycleBinCommandHandler> logger)
        {
            _recycleBinCleanupService = recycleBinCleanupService;
            _logger = logger;
        }

        public async Task<ResponseMessageDto<bool>> Handle(ClearRecycleBinCommand request, CancellationToken cancellationToken)
        {
            var response = new ResponseMessageDto<bool> { IsSuccess = true };

            try
            {
                await _recycleBinCleanupService.ClearAllAsync(cancellationToken);

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
    }
}
