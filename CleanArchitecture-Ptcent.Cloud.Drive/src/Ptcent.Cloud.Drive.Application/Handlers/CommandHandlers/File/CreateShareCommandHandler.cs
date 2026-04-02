using MediatR;
using Microsoft.Extensions.Logging;
using Ptcent.Cloud.Drive.Application.Attributes;
using Ptcent.Cloud.Drive.Application.Contracts.Responses;
using Ptcent.Cloud.Drive.Application.Features.Files.Commands;
using Ptcent.Cloud.Drive.Application.Interfaces;
using Ptcent.Cloud.Drive.Application.Interfaces.Persistence;
using Ptcent.Cloud.Drive.Domain.Entities;
using Ptcent.Cloud.Drive.Domain.Enum;
using System.Security.Cryptography;

namespace Ptcent.Cloud.Drive.Application.Handlers.CommandHandlers.File
{
    /// <summary>
    /// 创建分享命令处理器
    /// </summary>
    [Transactional]
    public class CreateShareCommandHandler : IRequestHandler<CreateShareCommand, ResponseMessageDto<ShareResultDto>>
    {
        private readonly IFileRepository _fileRepository;
        private readonly IShareRepository _shareRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<CreateShareCommandHandler> _logger;

        public CreateShareCommandHandler(
            IFileRepository fileRepository,
            IShareRepository shareRepository,
            IUserRepository userRepository,
            ILogger<CreateShareCommandHandler> logger)
        {
            _fileRepository = fileRepository;
            _shareRepository = shareRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<ResponseMessageDto<ShareResultDto>> Handle(CreateShareCommand request, CancellationToken cancellationToken)
        {
            var response = new ResponseMessageDto<ShareResultDto> { IsSuccess = true };

            try
            {
                // 1. 检查文件是否存在
                var file = await _fileRepository.GetByIdAsync(request.FileId, cancellationToken);
                if (file == null || file.IsDel == (int)FileStatsType.Del)
                {
                    response.IsSuccess = false;
                    response.Message = "文件不存在";
                    return response;
                }

                // 2. 生成唯一分享码
                string shareCode;
                do
                {
                    shareCode = GenerateShareCode();
                } while (await _shareRepository.ExistsByShareCodeAsync(shareCode));

                // 3. 创建分享记录
                var share = new ShareEntity
                {
                    ShareCode = shareCode,
                    FileId = request.FileId,
                    FileIdPath = file.Idpath,
                    UserId = await _userRepository.UserId(),
                    AccessPassword = string.IsNullOrEmpty(request.AccessPassword) ? null : request.AccessPassword,
                    ExpireTime = request.ExpireDays.HasValue ? DateTime.Now.AddDays(request.ExpireDays.Value) : null,
                    MaxVisitCount = request.MaxVisitCount ?? 0,
                    IsValid = 1,
                    CreateTime = DateTime.Now
                };

                await _shareRepository.AddAsync(share, cancellationToken);

                // 4. 返回分享结果
                var result = new ShareResultDto
                {
                    ShareId = share.Id,
                    ShareCode = shareCode,
                    ShareUrl = $"/s/{shareCode}",
                    ExpireTime = share.ExpireTime,
                    HasPassword = !string.IsNullOrEmpty(share.AccessPassword)
                };

                response.Data = result;
                response.Message = "分享创建成功";
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建分享失败：FileId={FileId}, Error={Error}", request.FileId, ex.Message);

                response.IsSuccess = false;
                response.Message = $"创建分享失败：{ex.Message}";
                return response;
            }
        }

        /// <summary>
        /// 生成分享码（8 位随机字符串）
        /// </summary>
        private string GenerateShareCode()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var result = new char[8];
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[1];
            for (int i = 0; i < 8; i++)
            {
                rng.GetBytes(bytes);
                result[i] = chars[bytes[0] % chars.Length];
            }
            return new string(result);
        }
    }
}
