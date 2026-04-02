using MediatR;
using Microsoft.Extensions.Logging;
using Ptcent.Cloud.Drive.Application.Attributes;
using Ptcent.Cloud.Drive.Application.Contracts.Responses;
using Ptcent.Cloud.Drive.Application.Features.Files.Commands;
using Ptcent.Cloud.Drive.Application.Interfaces;
using Ptcent.Cloud.Drive.Application.Interfaces.Persistence;
using Ptcent.Cloud.Drive.Domain.Enum;
using System.Linq;

namespace Ptcent.Cloud.Drive.Application.Handlers.CommandHandlers.File
{
    /// <summary>
    /// 取消分享命令处理器
    /// </summary>
    [Transactional]
    public class CancelShareCommandHandler : IRequestHandler<CancelShareCommand, ResponseMessageDto<bool>>
    {
        private readonly IShareRepository _shareRepository;
        private readonly ILogger<CancelShareCommandHandler> _logger;

        public CancelShareCommandHandler(
            IShareRepository shareRepository,
            ILogger<CancelShareCommandHandler> logger)
        {
            _shareRepository = shareRepository;
            _logger = logger;
        }

        public async Task<ResponseMessageDto<bool>> Handle(CancelShareCommand request, CancellationToken cancellationToken)
        {
            var response = new ResponseMessageDto<bool> { IsSuccess = true };

            try
            {
                // 获取分享记录
                var share = await _shareRepository.GetByIdAsync(request.ShareId, cancellationToken);
                if (share == null)
                {
                    response.IsSuccess = false;
                    response.Message = "分享记录不存在";
                    return response;
                }

                // 取消分享（标记为无效）
                share.IsValid = 0;
                share.UpdateTime = DateTime.Now;

                await _shareRepository.UpdateAsync(share, cancellationToken);

                response.Message = "取消分享成功";
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取消分享失败：ShareId={ShareId}, Error={Error}", request.ShareId, ex.Message);

                response.IsSuccess = false;
                response.Message = $"取消分享失败：{ex.Message}";
                return response;
            }
        }
    }

    /// <summary>
    /// 获取我的分享列表查询处理器
    /// </summary>
    public class GetMySharesQueryHandler : IRequestHandler<GetMySharesQuery, ResponseMessageDto<List<MyShareDto>>>
    {
        private readonly IShareRepository _shareRepository;
        private readonly IFileRepository _fileRepository;
        private readonly IUserRepository _userRepository;

        public GetMySharesQueryHandler(
            IShareRepository shareRepository,
            IFileRepository fileRepository,
            IUserRepository userRepository)
        {
            _shareRepository = shareRepository;
            _fileRepository = fileRepository;
            _userRepository = userRepository;
        }

        public async Task<ResponseMessageDto<List<MyShareDto>>> Handle(GetMySharesQuery request, CancellationToken cancellationToken)
        {
            var response = new ResponseMessageDto<List<MyShareDto>> { IsSuccess = true };

            try
            {
                var userId = await _userRepository.UserId();

                // 获取用户的分享列表
                var shares = await _shareRepository.QueryAsync(
                    s => s.UserId == userId,
                    cancellationToken
                );

                var shareList = shares.ToList();

                var result = new List<MyShareDto>();

                foreach (var share in shareList)
                {
                    var file = await _fileRepository.GetByIdAsync(share.FileId);

                    result.Add(new MyShareDto
                    {
                        ShareId = share.Id,
                        FileId = share.FileId,
                        FileName = file?.LeafName ?? "已删除文件",
                        IsFolder = file?.IsFolder == 1,
                        ShareCode = share.ShareCode,
                        ShareUrl = $"/s/{share.ShareCode}",
                        ExpireTime = share.ExpireTime,
                        HasPassword = !string.IsNullOrEmpty(share.AccessPassword),
                        VisitCount = share.VisitCount,
                        IsValid = share.IsValid == 1,
                        CreateTime = share.CreateTime
                    });
                }

                response.Data = result;
                return response;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = $"获取分享列表失败：{ex.Message}";
                return response;
            }
        }
    }
}
