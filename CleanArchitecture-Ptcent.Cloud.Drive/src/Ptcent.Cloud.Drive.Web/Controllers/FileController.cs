using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ptcent.Cloud.Drive.Application.Contracts.Responses;
using Ptcent.Cloud.Drive.Application.Dto.ReponseModels;
using Ptcent.Cloud.Drive.Application.Dto.RequestModels;
using Ptcent.Cloud.Drive.Application.Features.Files.Commands;
using Ptcent.Cloud.Drive.Application.Features.Files.Queries;
using Ptcent.Cloud.Drive.Application.Interfaces.Persistence;
using Ptcent.Cloud.Drive.Application.Services;
using Ptcent.Cloud.Drive.Domain.Constants;
using Ptcent.Cloud.Drive.Domain.Entities;
using System.Security.Cryptography;

namespace Ptcent.Cloud.Drive.Web.Controllers
{
    /// <summary>
    /// 文件管理接口
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FileController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly IShareRepository _shareRepository;
        private readonly ICacheService _cacheService;

        public FileController(
            IMediator mediator,
            IConfiguration config,
            IShareRepository shareRepository,
            ICacheService cacheService) : base(config)
        {
            _mediator = mediator;
            _shareRepository = shareRepository;
            _cacheService = cacheService;
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        [HttpPost("upload")]
        [RequestSizeLimit(100 * 1024 * 1024)]
        public async Task<ActionResult<ResponseMessageDto<UploadFileResult>>> UploadFile(IFormFile file, [FromForm] long? parentFolderId, [FromForm] string? filePath)
        {
            var command = new UploadFileCommand(file, parentFolderId, filePath);
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// 获取文件列表
        /// </summary>
        [HttpGet("list")]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseMessageDto<List<ItemResponseDto>>>> GetFileList([FromQuery] FileRequestDto request)
        {
            var result = await _mediator.Send(request);
            return Ok(result);
        }

        /// <summary>
        /// 创建文件夹
        /// </summary>
        [HttpPost("folder")]
        public async Task<ActionResult<ResponseMessageDto<bool>>> CreateFolder([FromBody] CreateFolderRequestDto request)
        {
            return await _mediator.Send(request);
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        [HttpDelete("{fileId}")]
        public async Task<ActionResult<ResponseMessageDto<bool>>> DeleteFile(long fileId)
        {
            var command = new DeleteFileCommand(fileId);
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// 重命名文件
        /// </summary>
        [HttpPut("{fileId}/rename")]
        public async Task<ActionResult<ResponseMessageDto<bool>>> RenameFile(long fileId, [FromBody] RenameFileRequest request)
        {
            var command = new RenameFileCommand(fileId, request.NewName);
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// 移动文件
        /// </summary>
        [HttpPost("move")]
        public async Task<ActionResult<ResponseMessageDto<bool>>> MoveFile([FromBody] MoveFileRequest request)
        {
            var command = new MoveFileCommand(request.FileId, request.NewParentFolderId);
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// 复制文件
        /// </summary>
        [HttpPost("copy")]
        public async Task<ActionResult<ResponseMessageDto<bool>>> CopyFile([FromBody] CopyFileRequest request)
        {
            var command = new CopyFileCommand(request.FileId, request.TargetParentFolderId);
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// 获取文件预览内容
        /// </summary>
        [HttpGet("{fileId}/preview")]
        public async Task<IActionResult> GetPreviewUrl(long fileId)
        {
            var query = new DownloadFileQuery(fileId);
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            var (fileStream, fileName, contentType, fileSize) = result.Data;

            if (contentType is "image/jpeg" or "image/png" or "image/gif" or "application/pdf" or "text/plain")
            {
                Response.Headers.Add("Content-Length", fileSize.ToString());
                return File(fileStream, contentType, fileName);
            }

            return BadRequest(new { message = "This file type is not supported for online preview.", fileName, fileType = contentType });
        }

        /// <summary>
        /// 获取回收站列表
        /// </summary>
        [HttpGet("recycle")]
        public async Task<ActionResult<ResponseMessageDto<List<ItemResponseDto>>>> GetRecycleBin([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            var query = new RecycleBinQuery(pageIndex, pageSize);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// 还原文件
        /// </summary>
        [HttpPost("recycle/{fileId}/restore")]
        public async Task<ActionResult<ResponseMessageDto<bool>>> RestoreFile(long fileId)
        {
            var command = new RestoreFileCommand(fileId);
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// 彻底删除文件
        /// </summary>
        [HttpDelete("recycle/{fileId}")]
        public async Task<ActionResult<ResponseMessageDto<bool>>> DeleteFilePermanently(long fileId)
        {
            var command = new DeleteFilePermanentlyCommand(fileId);
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// 清空回收站
        /// </summary>
        [HttpDelete("recycle")]
        public async Task<ActionResult<ResponseMessageDto<bool>>> ClearRecycleBin()
        {
            var command = new ClearRecycleBinCommand();
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// 创建分享
        /// </summary>
        [HttpPost("share")]
        public async Task<ActionResult<ResponseMessageDto<ShareResultDto>>> CreateShare([FromBody] CreateShareRequest request)
        {
            var command = new CreateShareCommand(request.FileId, request.AccessPassword, request.ExpireDays, request.MaxVisitCount);
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// 获取分享信息
        /// </summary>
        [HttpGet("share/{shareCode}")]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseMessageDto<ShareInfoDto>>> GetShareInfo(string shareCode)
        {
            var query = new GetShareInfoQuery(shareCode);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// 校验分享密码并发放访问令牌
        /// </summary>
        [HttpPost("share/{shareCode}/verify")]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseMessageDto<ShareAccessTokenDto>>> VerifyShareAccess(string shareCode, [FromBody] VerifySharePasswordRequest request)
        {
            var share = await GetActiveShareAsync(shareCode);
            if (share == null)
            {
                return Ok(new ResponseMessageDto<ShareAccessTokenDto>
                {
                    IsSuccess = false,
                    Message = "Share is unavailable."
                });
            }

            if (!string.IsNullOrEmpty(share.AccessPassword))
            {
                if (string.IsNullOrWhiteSpace(request.Password) ||
                    !string.Equals(share.AccessPassword, request.Password, StringComparison.Ordinal))
                {
                    return Ok(new ResponseMessageDto<ShareAccessTokenDto>
                    {
                        IsSuccess = false,
                        Message = "Invalid share password."
                    });
                }
            }

            var accessToken = await CreateShareAccessTokenAsync(shareCode, share.ExpireTime);
            return Ok(new ResponseMessageDto<ShareAccessTokenDto>
            {
                IsSuccess = true,
                Data = new ShareAccessTokenDto { AccessToken = accessToken },
                Message = "Verification succeeded."
            });
        }

        /// <summary>
        /// 公开下载分享文件
        /// </summary>
        [HttpGet("share/{shareCode}/download")]
        [AllowAnonymous]
        public async Task<IActionResult> DownloadSharedFile(string shareCode, [FromQuery] string? accessToken)
        {
            var share = await GetActiveShareAsync(shareCode);
            if (share == null)
            {
                return BadRequest(new ResponseMessageDto<bool> { IsSuccess = false, Message = "Share is unavailable." });
            }

            if (share.MaxVisitCount > 0 && share.VisitCount >= share.MaxVisitCount)
            {
                return BadRequest(new ResponseMessageDto<bool> { IsSuccess = false, Message = "Share visit limit reached." });
            }

            if (!string.IsNullOrEmpty(share.AccessPassword))
            {
                var isValid = await ValidateShareAccessTokenAsync(shareCode, accessToken);
                if (!isValid)
                {
                    return Unauthorized(new ResponseMessageDto<bool> { IsSuccess = false, Message = "Share access token is invalid." });
                }
            }

            var result = await _mediator.Send(new DownLoadFileRequestDto
            {
                FileIds = new[] { share.FileId }
            });

            if (!result.IsSuccess || string.IsNullOrWhiteSpace(result.Data.filePath) || !System.IO.File.Exists(result.Data.filePath))
            {
                return BadRequest(result);
            }

            share.VisitCount += 1;
            share.UpdateTime = DateTime.Now;
            await _shareRepository.UpdateAsync(share);

            var fileName = result.Data.fileName;
            var contentType = GetContentType(Path.GetExtension(fileName));
            return PhysicalFile(result.Data.filePath, contentType, fileName);
        }

        /// <summary>
        /// 取消分享
        /// </summary>
        [HttpDelete("share/{shareId}")]
        public async Task<ActionResult<ResponseMessageDto<bool>>> CancelShare(long shareId)
        {
            var command = new CancelShareCommand(shareId);
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// 获取我的分享列表
        /// </summary>
        [HttpGet("shares")]
        public async Task<ActionResult<ResponseMessageDto<List<MyShareDto>>>> GetMyShares([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            var query = new GetMySharesQuery(pageIndex, pageSize);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// 添加收藏
        /// </summary>
        [HttpPost("collection")]
        public async Task<ActionResult<ResponseMessageDto<bool>>> AddToCollection([FromBody] CollectionRequest request)
        {
            var command = new AddToCollectionCommand(request.FileId);
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// 取消收藏
        /// </summary>
        [HttpDelete("collection/{fileId}")]
        public async Task<ActionResult<ResponseMessageDto<bool>>> RemoveFromCollection(long fileId)
        {
            var command = new RemoveFromCollectionCommand(fileId);
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// 获取收藏列表
        /// </summary>
        [HttpGet("collections")]
        public async Task<ActionResult<ResponseMessageDto<List<CollectionItemDto>>>> GetCollections([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            var query = new GetCollectionsQuery(pageIndex, pageSize);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// 检查是否已收藏
        /// </summary>
        [HttpGet("collection/check/{fileId}")]
        public async Task<ActionResult<ResponseMessageDto<bool>>> CheckCollection(long fileId)
        {
            var result = new ResponseMessageDto<bool> { IsSuccess = true, Data = false };
            return Ok(result);
        }

        private async Task<ShareEntity?> GetActiveShareAsync(string shareCode)
        {
            var share = await _shareRepository.GetByShareCodeAsync(shareCode);
            if (share == null || share.IsValid != 1)
            {
                return null;
            }

            if (share.ExpireTime.HasValue && share.ExpireTime.Value <= DateTime.Now)
            {
                return null;
            }

            return share;
        }

        private async Task<string> CreateShareAccessTokenAsync(string shareCode, DateTime? expireTime)
        {
            var accessToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(24)).ToLowerInvariant();
            var cacheKey = string.Format(CacheKeys.ShareAccessToken, shareCode, accessToken);
            var expiration = CacheExpiration.ShareAccessToken;

            if (expireTime.HasValue)
            {
                var remaining = expireTime.Value - DateTime.Now;
                if (remaining > TimeSpan.Zero && remaining < expiration)
                {
                    expiration = remaining;
                }
            }

            await _cacheService.SetAsync(cacheKey, true, expiration);
            return accessToken;
        }

        private async Task<bool> ValidateShareAccessTokenAsync(string shareCode, string? accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return false;
            }

            var cacheKey = string.Format(CacheKeys.ShareAccessToken, shareCode, accessToken);
            return await _cacheService.ExistsAsync(cacheKey);
        }

        private static string GetContentType(string? extension)
        {
            return extension?.ToLowerInvariant() switch
            {
                ".zip" => "application/zip",
                ".pdf" => "application/pdf",
                ".txt" => "text/plain",
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".mp4" => "video/mp4",
                _ => "application/octet-stream"
            };
        }
    }

    #region 请求 DTO

    public class CreateShareRequest
    {
        public long FileId { get; set; }
        public string? AccessPassword { get; set; }
        public int? ExpireDays { get; set; }
        public int? MaxVisitCount { get; set; }
    }

    public class VerifySharePasswordRequest
    {
        public string? Password { get; set; }
    }

    public class ShareAccessTokenDto
    {
        public string AccessToken { get; set; } = string.Empty;
    }

    public class CollectionRequest
    {
        public long FileId { get; set; }
    }

    public class RenameFileRequest
    {
        public string NewName { get; set; } = string.Empty;
    }

    public class MoveFileRequest
    {
        public long FileId { get; set; }
        public long? NewParentFolderId { get; set; }
    }

    public class CopyFileRequest
    {
        public long FileId { get; set; }
        public long? TargetParentFolderId { get; set; }
    }

    #endregion
}
