using MediatR;
using Microsoft.Extensions.Configuration;
using Ptcent.Cloud.Drive.Application.Contracts.Responses;
using Ptcent.Cloud.Drive.Application.Interfaces;
using Ptcent.Cloud.Drive.Application.Interfaces.Persistence;
using Ptcent.Cloud.Drive.Application.Services;
using Ptcent.Cloud.Drive.Domain.Constants;

namespace Ptcent.Cloud.Drive.Application.Features.Files.Queries
{
    /// <summary>
    /// 下载文件查询处理器
    /// </summary>
    public class DownloadFileQueryHandler : IRequestHandler<DownloadFileQuery, ResponseMessageDto<(System.IO.FileStream FileStream, string FileName, string? ContentType, long FileSize)>>
    {
        private readonly IFileRepository _fileRepository;
        private readonly ICacheService _cacheService;
        private readonly IConfiguration _configuration;

        public DownloadFileQueryHandler(
            IFileRepository fileRepository,
            ICacheService cacheService,
            IConfiguration configuration)
        {
            _fileRepository = fileRepository;
            _cacheService = cacheService;
            _configuration = configuration;
        }

        public async Task<ResponseMessageDto<(System.IO.FileStream FileStream, string FileName, string? ContentType, long FileSize)>> Handle(DownloadFileQuery request, CancellationToken cancellationToken)
        {
            var response = new ResponseMessageDto<(System.IO.FileStream FileStream, string FileName, string? ContentType, long FileSize)>();

            try
            {
                // 先从缓存获取
                var cacheKey = string.Format(CacheKeys.FileMetadata, request.FileId);
                var fileEntity = await _cacheService.GetAsync<Domain.Entities.FileEntity>(cacheKey, cancellationToken);

                if (fileEntity == null)
                {
                    // 从数据库获取
                    fileEntity = await _fileRepository.FirstOrDefaultAsync(
                        f => f.Id == request.FileId && f.IsDel == 0,
                        cancellationToken
                    );

                    if (fileEntity == null)
                    {
                        response.IsSuccess = false;
                        response.Code = WebApiResultCode.NotFound;
                        response.Message = "文件不存在或已被删除";
                        return response;
                    }

                    // 缓存文件信息
                    await _cacheService.SetAsync(cacheKey, fileEntity, CacheExpiration.FileMetadata, cancellationToken);
                }

                // 检查文件是否存在
                if (!System.IO.File.Exists(fileEntity.PhysicalDirectory))
                {
                    response.IsSuccess = false;
                    response.Code = WebApiResultCode.NotFound;
                    response.Message = "文件物理路径不存在";
                    return response;
                }

                // 打开文件流
                var fileStream = new FileStream(fileEntity.PhysicalDirectory, FileMode.Open, FileAccess.Read, FileShare.Read);

                // 获取 Content-Type
                var contentType = GetContentType(fileEntity.Extension);

                response.IsSuccess = true;
                response.Code = WebApiResultCode.Success;
                response.Message = "获取文件成功";
                response.Data = (fileStream, fileEntity.LeafName, contentType, fileEntity.FileSize ?? 0);

                return response;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Code = WebApiResultCode.SystemError;
                response.Message = $"下载失败：{ex.Message}";
                return response;
            }
        }

        private static string? GetContentType(string? extension)
        {
            return extension?.ToLower() switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".ppt" => "application/vnd.ms-powerpoint",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".mp4" => "video/mp4",
                ".avi" => "video/x-msvideo",
                ".mov" => "video/quicktime",
                ".mp3" => "audio/mpeg",
                ".wav" => "audio/wav",
                ".zip" => "application/zip",
                ".rar" => "application/vnd.rar",
                ".txt" => "text/plain",
                _ => "application/octet-stream"
            };
        }
    }
}
