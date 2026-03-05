using MediatR;
using Ptcent.Cloud.Drive.Application.Contracts.Responses;
using Ptcent.Cloud.Drive.Application.Interfaces;
using Ptcent.Cloud.Drive.Application.Services;
using Ptcent.Cloud.Drive.Domain.Constants;
using System.IO.Compression;

namespace Ptcent.Cloud.Drive.Application.Features.Files.Queries
{
    /// <summary>
    /// 批量下载查询处理器
    /// </summary>
    public class BatchDownloadQueryHandler : IRequestHandler<BatchDownloadQuery, ResponseMessageDto<(System.IO.MemoryStream ZipStream, string ZipFileName)>>
    {
        private readonly IFileRepository _fileRepository;
        private readonly ICacheService _cacheService;

        public BatchDownloadQueryHandler(
            IFileRepository fileRepository,
            ICacheService cacheService)
        {
            _fileRepository = fileRepository;
            _cacheService = cacheService;
        }

        public async Task<ResponseMessageDto<(System.IO.MemoryStream ZipStream, string ZipFileName)>> Handle(BatchDownloadQuery request, CancellationToken cancellationToken)
        {
            var response = new ResponseMessageDto<(System.IO.MemoryStream ZipStream, string ZipFileName)>();

            try
            {
                if (request.FileIds == null || request.FileIds.Length == 0)
                {
                    response.IsSuccess = false;
                    response.Code = WebApiResultCode.BadRequest;
                    response.Message = "文件 ID 不能为空";
                    return response;
                }

                // 获取所有文件
                var files = new List<Domain.Entities.FileEntity>();
                foreach (var fileId in request.FileIds)
                {
                    var cacheKey = string.Format(CacheKeys.FileMetadata, fileId);
                    var file = await _cacheService.GetAsync<Domain.Entities.FileEntity>(cacheKey, cancellationToken);

                    if (file == null)
                    {
                        file = await _fileRepository.FirstOrDefaultAsync(
                            f => f.Id == fileId && f.IsDel == 0 && f.IsFolder == 0,
                            cancellationToken
                        );

                        if (file != null)
                        {
                            await _cacheService.SetAsync(cacheKey, file, CacheExpiration.FileMetadata, cancellationToken);
                        }
                    }

                    if (file != null && System.IO.File.Exists(file.PhysicalDirectory))
                    {
                        files.Add(file);
                    }
                }

                if (files.Count == 0)
                {
                    response.IsSuccess = false;
                    response.Code = WebApiResultCode.NotFound;
                    response.Message = "没有找到可下载的文件";
                    return response;
                }

                // 创建 ZIP 压缩包
                var memoryStream = new MemoryStream();
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    foreach (var file in files)
                    {
                        // 避免文件名冲突
                        var entryName = GetUniqueFileName(archive.Entries, file.LeafName);

                        var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
                        using var entryStream = entry.Open();
                        using var fileStream = new FileStream(file.PhysicalDirectory, FileMode.Open, FileAccess.Read);
                        await fileStream.CopyToAsync(entryStream, 81920, cancellationToken);
                    }
                }

                memoryStream.Position = 0;

                var zipFileName = $"download_{DateTime.UtcNow:yyyyMMddHHmmss}.zip";

                response.IsSuccess = true;
                response.Code = WebApiResultCode.Success;
                response.Message = "打包成功";
                response.Data = (memoryStream, zipFileName);

                return response;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Code = WebApiResultCode.SystemError;
                response.Message = $"打包失败：{ex.Message}";
                return response;
            }
        }

        private static string GetUniqueFileName(IReadOnlyList<ZipArchiveEntry> entries, string fileName)
        {
            var existingNames = entries.Select(e => e.FullName).ToHashSet();

            if (!existingNames.Contains(fileName))
            {
                return fileName;
            }

            var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            var extension = Path.GetExtension(fileName);
            var counter = 1;

            while (existingNames.Contains($"{nameWithoutExt} ({counter}){extension}"))
            {
                counter++;
            }

            return $"{nameWithoutExt} ({counter}){extension}";
        }
    }
}
