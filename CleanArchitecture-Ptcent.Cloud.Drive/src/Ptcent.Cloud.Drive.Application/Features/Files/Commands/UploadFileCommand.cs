using MediatR;
using Microsoft.AspNetCore.Http;
using Ptcent.Cloud.Drive.Application.Contracts.Responses;

namespace Ptcent.Cloud.Drive.Application.Features.Files.Commands
{
    /// <summary>
    /// 上传文件命令
    /// </summary>
    public record UploadFileCommand(
        IFormFile File,
        long? ParentFolderId,
        string? FilePath
    ) : IRequest<ResponseMessageDto<UploadFileResult>>;

    /// <summary>
    /// 上传文件结果
    /// </summary>
    public class UploadFileResult
    {
        public long FileId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string FileType { get; set; } = string.Empty;
    }
}
