using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ptcent.Cloud.Drive.Application.Contracts.Responses;
using Ptcent.Cloud.Drive.Application.Dto.ReponseModels;
using Ptcent.Cloud.Drive.Application.Interfaces;
using Ptcent.Cloud.Drive.Application.Interfaces.Persistence;

namespace Ptcent.Cloud.Drive.Web.Controllers
{
    /// <summary>
    /// 系统管理接口
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SystemController : ControllerBase
    {
        private readonly IOperationLogRepository _operationLogRepository;
        private readonly IUserRepository _userRepository;
        private readonly IRecycleBinCleanupService _recycleBinCleanupService;
        private readonly IStorageStatsService _storageStatsService;

        public SystemController(
            IOperationLogRepository operationLogRepository,
            IUserRepository userRepository,
            IRecycleBinCleanupService recycleBinCleanupService,
            IStorageStatsService storageStatsService)
        {
            _operationLogRepository = operationLogRepository;
            _userRepository = userRepository;
            _recycleBinCleanupService = recycleBinCleanupService;
            _storageStatsService = storageStatsService;
        }

        /// <summary>
        /// 获取当前用户的操作日志
        /// </summary>
        [HttpGet("operation-logs")]
        public async Task<ActionResult<ResponseMessageDto<List<OperationLogItemDto>>>> GetOperationLogs([FromQuery] OperationLogQueryRequest request, CancellationToken cancellationToken)
        {
            var userId = await _userRepository.UserId();
            var (items, total) = await _operationLogRepository.GetPagedAsync(
                userId,
                request.Keyword,
                request.StartTime,
                request.EndTime,
                request.PageIndex,
                request.PageSize,
                cancellationToken);

            var response = new ResponseMessageDto<List<OperationLogItemDto>>
            {
                IsSuccess = true,
                Data = items.Select(x => new OperationLogItemDto
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    UserName = x.UserName,
                    Phone = x.Phone,
                    Method = x.Method,
                    Path = x.Path,
                    ActionName = x.ActionName,
                    IpAddress = x.IpAddress,
                    RequestQuery = x.RequestQuery,
                    StatusCode = x.StatusCode,
                    IsSuccess = x.IsSuccess,
                    DurationMs = x.DurationMs,
                    ErrorMessage = x.ErrorMessage,
                    CreatedTime = x.CreatedTime
                }).ToList(),
                TotalCount = total,
                Message = "获取成功"
            };

            return Ok(response);
        }

        /// <summary>
        /// 获取存储统计
        /// </summary>
        [HttpGet("storage-stats")]
        public async Task<ActionResult<ResponseMessageDto<StorageStatsDto>>> GetStorageStats(CancellationToken cancellationToken)
        {
            var userId = await _userRepository.UserId();
            var stats = await _storageStatsService.GetStatsAsync(userId, cancellationToken);

            return Ok(new ResponseMessageDto<StorageStatsDto>
            {
                IsSuccess = true,
                Data = stats,
                Message = "获取成功"
            });
        }

        /// <summary>
        /// 手动清理过期回收站文件
        /// </summary>
        [HttpPost("recycle/cleanup-expired")]
        public async Task<ActionResult<ResponseMessageDto<int>>> CleanupExpiredRecycleBin(CancellationToken cancellationToken)
        {
            var deletedCount = await _recycleBinCleanupService.CleanupExpiredAsync(cancellationToken);
            return Ok(new ResponseMessageDto<int>
            {
                IsSuccess = true,
                Data = deletedCount,
                Message = $"清理完成，共删除 {deletedCount} 条回收站记录"
            });
        }
    }

    public class OperationLogQueryRequest
    {
        public string? Keyword { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
