using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ptcent.Cloud.Drive.Application.Contracts.Responses;
using Ptcent.Cloud.Drive.Application.Features.Files.Commands;
using Ptcent.Cloud.Drive.Application.Features.Files.Queries;

namespace Ptcent.Cloud.Drive.Web.Controllers
{
    /// <summary>
    /// 文件管理接口
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FileController : ControllerBase
    {
        private readonly IMediator _mediator;

        public FileController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        [HttpPost("upload")]
        [RequestSizeLimit(100 * 1024 * 1024)] // 100MB
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
        public async Task<ActionResult<ResponseMessageDto<List<FileDto>>>> GetFileList(long? parentFolderId)
        {
            // TODO: 实现 GetFileListQuery
            return Ok(new ResponseMessageDto<List<FileDto>>
            {
                IsSuccess = true,
                Message = "获取成功",
                Data = new List<FileDto>()
            });
        }

        /// <summary>
        /// 创建文件夹
        /// </summary>
        [HttpPost("folder")]
        public async Task<ActionResult<ResponseMessageDto<bool>>> CreateFolder([FromBody] CreateFolderRequest request)
        {
            // TODO: 实现 CreateFolderCommand
            return Ok(new ResponseMessageDto<bool>
            {
                IsSuccess = true,
                Message = "创建成功",
                Data = true
            });
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
            // 返回文件信息，由前端决定如何预览
            var query = new DownloadFileQuery(fileId);
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            var (fileStream, fileName, contentType, fileSize) = result.Data;

            // 对于图片和 PDF 等可直接预览的文件，返回文件内容
            if (contentType is "image/jpeg" or "image/png" or "image/gif" or "application/pdf" or "text/plain")
            {
                Response.Headers.Add("Content-Length", fileSize.ToString());
                return File(fileStream, contentType, fileName);
            }

            // 其他文件类型返回下载链接
            return BadRequest(new { message = "该文件类型不支持在线预览", fileName, fileType = contentType });
        }
    }

    #region 请求 DTO

    public class CreateFolderRequest
    {
        public string FolderName { get; set; } = string.Empty;
        public long? ParentFolderId { get; set; }
        public string? Path { get; set; }
    }

    public class RenameFileRequest
    {
        public string NewName { get; set; } = string.Empty;
    }

    public class MoveFileRequest
    {
        public long FileId { get; set; }
        public long NewParentFolderId { get; set; }
    }

    public class FileDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Extension { get; set; }
        public bool IsFolder { get; set; }
        public long? Size { get; set; }
        public string? FileType { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? PreviewUrl { get; set; }
    }

    #endregion
}
