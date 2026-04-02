using MediatR;
using Microsoft.Extensions.Logging;
using Ptcent.Cloud.Drive.Application.Contracts.Responses;
using Ptcent.Cloud.Drive.Application.Features.Files.Queries;
using Ptcent.Cloud.Drive.Application.Interfaces.Persistence;
using Ptcent.Cloud.Drive.Domain.Enum;

namespace Ptcent.Cloud.Drive.Application.Handlers.QueryHandlers.File
{
    /// <summary>
    /// 获取分享信息查询处理器
    /// </summary>
    public class GetShareInfoQueryHandler : IRequestHandler<GetShareInfoQuery, ResponseMessageDto<ShareInfoDto>>
    {
        private readonly IShareRepository _shareRepository;
        private readonly IFileRepository _fileRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<GetShareInfoQueryHandler> _logger;

        public GetShareInfoQueryHandler(
            IShareRepository shareRepository,
            IFileRepository fileRepository,
            IUserRepository userRepository,
            ILogger<GetShareInfoQueryHandler> logger)
        {
            _shareRepository = shareRepository;
            _fileRepository = fileRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<ResponseMessageDto<ShareInfoDto>> Handle(GetShareInfoQuery request, CancellationToken cancellationToken)
        {
            var response = new ResponseMessageDto<ShareInfoDto> { IsSuccess = true };

            try
            {
                // 1. 获取分享信息
                var share = await _shareRepository.GetByShareCodeAsync(request.ShareCode);
                if (share == null)
                {
                    response.IsSuccess = false;
                    response.Message = "分享不存在";
                    return response;
                }

                // 2. 检查分享是否有效
                if (share.IsValid != 1)
                {
                    return new ResponseMessageDto<ShareInfoDto>
                    {
                        IsSuccess = true,
                        Data = new ShareInfoDto { IsValid = false, IsExpired = false }
                    };
                }

                // 3. 检查是否过期
                if (share.ExpireTime.HasValue && share.ExpireTime < DateTime.Now)
                {
                    return new ResponseMessageDto<ShareInfoDto>
                    {
                        IsSuccess = true,
                        Data = new ShareInfoDto { IsValid = true, IsExpired = true }
                    };
                }

                // 4. 检查访问次数
                if (share.MaxVisitCount > 0 && share.VisitCount >= share.MaxVisitCount)
                {
                    return new ResponseMessageDto<ShareInfoDto>
                    {
                        IsSuccess = true,
                        Data = new ShareInfoDto { IsValid = false, IsExpired = false }
                    };
                }

                // 5. 获取文件信息
                var file = await _fileRepository.GetByIdAsync(share.FileId, cancellationToken);
                if (file == null)
                {
                    response.IsSuccess = false;
                    response.Message = "文件不存在";
                    return response;
                }

                // 6. 获取分享人信息
                var user = await _userRepository.GetByIdAsync(share.UserId);
                var userName = user?.UserName ?? "匿名用户";

                // 7. 返回分享信息
                response.Data = new ShareInfoDto
                {
                    FileId = share.FileId,
                    FileName = file.LeafName,
                    IsFolder = file.IsFolder == 1,
                    FileSize = file.FileSize ?? 0,
                    UserName = userName,
                    ExpireTime = share.ExpireTime,
                    HasPassword = !string.IsNullOrEmpty(share.AccessPassword),
                    IsExpired = false,
                    IsValid = true
                };

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取分享信息失败：ShareCode={ShareCode}, Error={Error}", request.ShareCode, ex.Message);

                response.IsSuccess = false;
                response.Message = $"获取分享信息失败：{ex.Message}";
                return response;
            }
        }
    }
}
