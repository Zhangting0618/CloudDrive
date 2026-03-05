using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ptcent.Cloud.Drive.Application.Contracts.Responses;
using Ptcent.Cloud.Drive.Application.Dto.ReponseModels;
using Ptcent.Cloud.Drive.Application.Dto.RequestModels;
using Ptcent.Cloud.Drive.Application.Features.Files.Commands;
using Ptcent.Cloud.Drive.Application.Features.Files.Queries;
using Ptcent.Cloud.Drive.Application.Handlers.CommandHandlers.File;

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

        public FileController(IMediator mediator, IConfiguration config) : base(config)
        {
            _mediator = mediator;
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
            // TODO: 实现 DeleteFileCommand
            return Ok(new ResponseMessageDto<bool>
            {
                IsSuccess = true,
                Message = "删除成功",
                Data = true
            });
        }

        /// <summary>
        /// 重命名文件
        /// </summary>
        [HttpPut("{fileId}/rename")]
        public async Task<ActionResult<ResponseMessageDto<bool>>> RenameFile(long fileId, [FromBody] RenameFileRequest request)
        {
            // TODO: 实现 RenameFileCommand
            return Ok(new ResponseMessageDto<bool>
            {
                IsSuccess = true,
                Message = "重命名成功",
                Data = true
            });
        }

        /// <summary>
        /// 移动文件
        /// </summary>
        [HttpPost("move")]
        public async Task<ActionResult<ResponseMessageDto<bool>>> MoveFile([FromBody] MoveFileRequest request)
        {
            // TODO: 实现 MoveFileCommand
            return Ok(new ResponseMessageDto<bool>
            {
                IsSuccess = true,
                Message = "移动成功",
                Data = true
            });
        }

        /// <summary>
        /// 获取文件预览 URL（前端预览）
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

            return BadRequest(new { message = "该文件类型不支持在线预览", fileName, fileType = contentType });
        }
    }

    #region 请求 DTO

    public class RenameFileRequest
    {
        public string NewName { get; set; } = string.Empty;
    }

    public class MoveFileRequest
    {
        public long FileId { get; set; }
        public long NewParentFolderId { get; set; }
    }

    #endregion
}
