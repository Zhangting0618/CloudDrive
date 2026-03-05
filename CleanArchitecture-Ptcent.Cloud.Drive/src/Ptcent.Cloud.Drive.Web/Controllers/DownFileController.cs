using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ptcent.Cloud.Drive.Application.Features.Files.Queries;

namespace Ptcent.Cloud.Drive.Web.Controllers
{
    /// <summary>
    /// 文件下载接口
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DownFileController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DownFileController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// 单个文件下载
        /// </summary>
        [HttpGet("download/{fileId}")]
        public async Task<IActionResult> DownloadFile(long fileId)
        {
            var query = new DownloadFileQuery(fileId);
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            var (fileStream, fileName, contentType, fileSize) = result.Data;

            Response.Headers.Append("Content-Length", fileSize.ToString());
            Response.Headers.Add("Accept-Ranges", "bytes");
            Response.Headers.Add("Content-Disposition", $"attachment; filename*=UTF-8''{Uri.EscapeDataString(fileName)}");

            return File(fileStream, contentType ?? "application/octet-stream", fileName);
        }

        /// <summary>
        /// 批量下载（打包为 ZIP）
        /// </summary>
        [HttpPost("download/batch")]
        public async Task<IActionResult> BatchDownload([FromBody] BatchDownloadRequest request)
        {
            var query = new BatchDownloadQuery(request.FileIds);
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            var (zipStream, zipFileName) = result.Data;

            Response.Headers.Add("Content-Disposition", $"attachment; filename*=UTF-8''{Uri.EscapeDataString(zipFileName)}");

            return File(zipStream, "application/zip", zipFileName);
        }
    }

    /// <summary>
    /// 批量下载请求
    /// </summary>
    public class BatchDownloadRequest
    {
        public long[] FileIds { get; set; } = Array.Empty<long>();
    }
}
