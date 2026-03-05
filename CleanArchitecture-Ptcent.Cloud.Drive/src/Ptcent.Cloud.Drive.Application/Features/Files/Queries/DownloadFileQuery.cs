using MediatR;
using Ptcent.Cloud.Drive.Application.Contracts.Responses;

namespace Ptcent.Cloud.Drive.Application.Features.Files.Queries
{
    /// <summary>
    /// 下载文件查询
    /// </summary>
    public record DownloadFileQuery(long FileId) : IRequest<ResponseMessageDto<(System.IO.FileStream FileStream, string FileName, string? ContentType, long FileSize)>>;

    /// <summary>
    /// 批量下载查询
    /// </summary>
    public record BatchDownloadQuery(long[] FileIds) : IRequest<ResponseMessageDto<(System.IO.MemoryStream ZipStream, string ZipFileName)>>;
}
