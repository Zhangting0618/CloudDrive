using MediatR;
using Ptcent.Cloud.Drive.Application.Contracts.Responses;
using Ptcent.Cloud.Drive.Application.Interfaces;
using Ptcent.Cloud.Drive.Application.Interfaces.Persistence;
using Ptcent.Cloud.Drive.Application.Services;
using Ptcent.Cloud.Drive.Domain.Entities;
using Ptcent.Cloud.Drive.Domain.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Ptcent.Cloud.Drive.Application.Features.Files.Commands
{
    /// <summary>
    /// 上传文件命令处理器
    /// </summary>
    public class UploadFileCommandHandler : IRequestHandler<UploadFileCommand, ResponseMessageDto<UploadFileResult>>
    {
        private readonly IFileRepository _fileRepository;
        private readonly IIdGeneratorService _idGenerator;
        private readonly ICacheService _cacheService;
        private readonly IConfiguration _configuration;
        private readonly IFileHashService _fileHashService;

        public UploadFileCommandHandler(
            IFileRepository fileRepository,
            IIdGeneratorService idGenerator,
            ICacheService cacheService,
            IConfiguration configuration,
            IFileHashService fileHashService)
        {
            _fileRepository = fileRepository;
            _idGenerator = idGenerator;
            _cacheService = cacheService;
            _configuration = configuration;
            _fileHashService = fileHashService;
        }

        public async Task<ResponseMessageDto<UploadFileResult>> Handle(UploadFileCommand request, CancellationToken cancellationToken)
        {
            var response = new ResponseMessageDto<UploadFileResult>();

            try
            {
                // 验证文件
                if (request.File == null || request.File.Length == 0)
                {
                    response.IsSuccess = false;
                    response.Code = WebApiResultCode.BadRequest;
                    response.Message = "文件不能为空";
                    return response;
                }

                // 检查文件大小
                var maxFileSize = _configuration.GetValue<long>("FileStorage:MaxFileSizeBytes");
                if (request.File.Length > maxFileSize)
                {
                    response.IsSuccess = false;
                    response.Code = WebApiResultCode.BadRequest;
                    response.Message = $"文件大小超过限制 ({maxFileSize / 1024 / 1024}MB)";
                    return response;
                }

                // 检查文件扩展名
                var allowedExtensions = _configuration.GetSection("FileStorage:AllowedExtensions").Get<string[]>() ?? Array.Empty<string>();
                var extension = Path.GetExtension(request.File.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    response.IsSuccess = false;
                    response.Code = WebApiResultCode.BadRequest;
                    response.Message = "不支持的文件类型";
                    return response;
                }

                // 计算文件 Hash（用于秒传）
                var fileHash = await _fileHashService.ComputeHashAsync(request.File.OpenReadStream(), cancellationToken);

                // 检查文件是否已存在（秒传）
                var existingFile = await _fileRepository.FirstOrDefaultAsync(
                    f => f.ItemHash == fileHash && f.IsDel == 0,
                    cancellationToken
                );

                if (existingFile != null)
                {
                    // 秒传成功
                    response.IsSuccess = true;
                    response.Code = WebApiResultCode.Success;
                    response.Message = "文件秒传成功";
                    response.Data = new UploadFileResult
                    {
                        FileId = existingFile.Id,
                        FileName = existingFile.LeafName,
                        FilePath = existingFile.Path,
                        FileSize = existingFile.FileSize ?? 0,
                        FileType = GetFileType(extension)
                    };
                    return response;
                }

                // 生成文件 ID 和存储路径
                var fileId = _idGenerator.NewId();
                var rootPath = _configuration["FileStorage:RootPath"] ?? "D:\\FileStorage";
                var datePath = DateTime.UtcNow.ToString("yyyy/MM/dd");
                var physicalPath = Path.Combine(rootPath, datePath);

                // 确保目录存在
                Directory.CreateDirectory(physicalPath);

                // 生成唯一文件名
                var fileName = $"{fileId}{extension}";
                var fullPath = Path.Combine(physicalPath, fileName);

                // 保存文件
                await using (var stream = System.IO.File.Create(fullPath))
                {
                    await request.File.CopyToAsync(stream, cancellationToken);
                }

                // 创建文件记录
                var fileEntity = new FileEntity
                {
                    Id = fileId,
                    LeafName = request.File.FileName,
                    Extension = extension,
                    Path = request.FilePath ?? "/",
                    ParentFolderId = request.ParentFolderId,
                    Idpath = $"/{fileId}",
                    IsFolder = 0,
                    FileType = GetFileTypeCode(extension),
                    IsDel = 0,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow,
                    CreatedBy = 0, // TODO: 从当前用户获取
                    UpdatedBy = 0,
                    VersionId = 1,
                    ItemHash = fileHash,
                    FileSize = request.File.Length,
                    PhysicalDirectory = fullPath,
                    ItemFileMapUrl = $"/api/files/{fileId}/preview"
                };

                var added = await _fileRepository.AddAsync(fileEntity, cancellationToken);

                if (!added)
                {
                    // 删除已保存的文件
                    if (System.IO.File.Exists(fullPath))
                        System.IO.File.Delete(fullPath);

                    response.IsSuccess = false;
                    response.Code = WebApiResultCode.SystemError;
                    response.Message = "文件上传失败";
                    return response;
                }

                // 缓存文件信息
                await _cacheService.SetAsync(
                    string.Format(CacheKeys.FileMetadata, fileId),
                    fileEntity,
                    CacheExpiration.FileMetadata,
                    cancellationToken
                );

                response.IsSuccess = true;
                response.Code = WebApiResultCode.Success;
                response.Message = "文件上传成功";
                response.Data = new UploadFileResult
                {
                    FileId = fileId,
                    FileName = request.File.FileName,
                    FilePath = fileEntity.Path,
                    FileSize = request.File.Length,
                    FileType = GetFileType(extension)
                };

                return response;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Code = WebApiResultCode.SystemError;
                response.Message = $"上传失败：{ex.Message}";
                return response;
            }
        }

        private static string GetFileType(string extension)
        {
            return extension.ToLower() switch
            {
                ".pdf" => "PDF 文档",
                ".doc" or ".docx" => "Word 文档",
                ".xls" or ".xlsx" => "Excel 表格",
                ".ppt" or ".pptx" => "PowerPoint 演示文稿",
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" => "图片",
                ".mp4" or ".avi" or ".mov" => "视频",
                ".mp3" or ".wav" => "音频",
                ".zip" or ".rar" or ".7z" => "压缩包",
                ".txt" => "文本文件",
                _ => "其他文件"
            };
        }

        private static int GetFileTypeCode(string extension)
        {
            return extension.ToLower() switch
            {
                ".pdf" or ".doc" or ".docx" or ".xls" or ".xlsx" or ".ppt" or ".pptx" or ".txt" => 1, // 文档
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" => 2, // 图片
                ".mp4" or ".avi" or ".mov" or ".wmv" => 3, // 视频
                ".mp3" or ".wav" or ".flac" => 4, // 音频
                ".zip" or ".rar" or ".7z" => 5, // 压缩包
                _ => 0 // 其他
            };
        }
    }
}
