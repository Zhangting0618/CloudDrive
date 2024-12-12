using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ptcent.Cloud.Drive.Application.Dto.RequestModels;
using Ptcent.Cloud.Drive.Shared.Util;
using SharpCompress.Common;
using System.Net.Http.Headers;

namespace Ptcent.Cloud.Drive.Web.Controllers
{
    /// <summary>
    /// 下载
    /// </summary>
    public class DownFileController : BaseController
    {
        private readonly IConfiguration config;
        private readonly IMediator mediator;
        /// <summary>
        /// 注入
        /// </summary>
        /// <param name="config"></param>
        /// <param name="mediator"></param>
        public DownFileController(IConfiguration config, IMediator mediator) : base(config)
        {
            this.config = config;
            this.mediator = mediator;
        }
        /// <summary>
        /// 单个下载
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> DownOnlyOfficeItem(long fileId)
        {
           var downOnlyOfficeItemRequestDto=new DownOnlyOfficeItemRequestDto { FileId = fileId };
            var (fileType, stream, archiveName) = await mediator.Send(downOnlyOfficeItemRequestDto);
            return File(stream, fileType, archiveName);
        }
        /// <summary>
        /// 文件/文件夹下载
        /// </summary>
        /// <param name="fileIds"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> DownLoadFile(long[] fileIds)
        {
            var downLoadFileRequestDto=new DownLoadFileRequestDto {  FileIds = fileIds };
            // 创建一个新的HeaderDictionary
            //var headers = new HeaderDictionary();
            var response= await mediator.Send(downLoadFileRequestDto);
            if (!response.IsSuccess)
            {
                var downloadFileResponse = new { IsSuccess=false, Code= 200, Message =$"文件下载失败:{response.Message}"};
                var mesages = downloadFileResponse.ToJson();
                return Content(mesages, "application/json");
            }
            //代表成功
            var (fileSourcePath, fileName) = response.Data;
            FileStream fileStream = new FileStream(fileSourcePath, FileMode.Open, FileAccess.Read);
            long fileSize = fileStream.Length;
            long startRange = 0;
            long endRange = fileSize - 1;
            if (Request.Headers.ContainsKey("Range"))
            {
                string rangeHeader = Request.Headers["Range"].ToString().Replace("bytes=", "");
                string[] rangeValues = rangeHeader.Split('-');
                startRange = long.Parse(rangeValues[0]);
                if (rangeValues[1] != "")
                    endRange = long.Parse(rangeValues[1]);
            }
            long contentLength = endRange - startRange + 1;
            Response.StatusCode = 206;
            string encodedFileName = Uri.EscapeDataString(fileName);
            if (fileName.Contains(".zip"))
            {
                Response.ContentType = "application/zip";
            }
            else
            {
                Response.ContentType = "application/octet-stream";
            }
           
            Response.Headers.Append("Content-Length", contentLength.ToString());
            Response.Headers.Append("Content-Range", $"bytes {startRange}-{endRange}/{fileSize}");
            Response.Headers.Append("Accept-Ranges", "bytes");
            Response.Headers.Append("Content-Disposition", $"attachment; filename*=UTF-8''{encodedFileName}");
            fileStream.Seek(startRange, SeekOrigin.Begin);
            byte[] buffer = new byte[1024 * 1024*5]; // 5MB buffer size
            while (contentLength > 0)
            {
                int bytesRead = fileStream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                    break;
                int bytesToWrite = (int)Math.Min(contentLength, bytesRead);
               await Response.Body.WriteAsync(buffer, 0, bytesToWrite);
                await Response.Body.FlushAsync(); //注意每次Write后，要及时调用Flush方法，及时释放服务器内存空间
                contentLength -= bytesToWrite;
            }
            fileStream.Close();
            return new EmptyResult();
        }
    }
}
