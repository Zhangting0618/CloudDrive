using AutoMapper;
using MediatR;
using Ptcent.Cloud.Drive.Application.Contracts.Responses;
using Ptcent.Cloud.Drive.Application.Dto.ReponseModels;
using Ptcent.Cloud.Drive.Application.Dto.RequestModels;
using Ptcent.Cloud.Drive.Application.Interfaces.Persistence;
using Ptcent.Cloud.Drive.Domain.Entities;
using Ptcent.Cloud.Drive.Domain.Enum;
using Ptcent.Cloud.Drive.Shared.Util;
using System.Linq.Expressions;

namespace Ptcent.Cloud.Drive.Application.Handlers.QueryHandlers.File
{
    internal class GetFilesQueryHandler : IRequestHandler<FileRequestDto, ResponseMessageDto<List<ItemResponseDto>>>
    {
        private readonly IFileRepository _fileRepository;
        private readonly IMapper _mapper;

        public GetFilesQueryHandler(IFileRepository fileRepository, IMapper mapper)
        {
            _fileRepository = fileRepository;
            _mapper = mapper;
        }

        public async Task<ResponseMessageDto<List<ItemResponseDto>>> Handle(FileRequestDto request, CancellationToken cancellationToken)
        {
            var response = new ResponseMessageDto<List<ItemResponseDto>> { IsSuccess = true };

            // 构建查询条件
            Expression<Func<FileEntity, bool>> where;

            if (request.ParentFolderId.HasValue)
            {
                var parentFolderId = request.ParentFolderId.Value;
                var fileType = (int)request.FileType;
                var keyword = request.KeyWord;

                if (request.FileType != FileTypeEnum.All && !string.IsNullOrWhiteSpace(keyword))
                {
                    where = a => a.IsDel == (int)FileStatsType.NoDel
                        && a.ParentFolderId == parentFolderId
                        && a.FileType == fileType
                        && a.LeafName.Contains(keyword);
                }
                else if (request.FileType != FileTypeEnum.All)
                {
                    where = a => a.IsDel == (int)FileStatsType.NoDel
                        && a.ParentFolderId == parentFolderId
                        && a.FileType == fileType;
                }
                else if (!string.IsNullOrWhiteSpace(keyword))
                {
                    // 优化搜索：支持文件名和扩展名搜索
                    where = a => a.IsDel == (int)FileStatsType.NoDel
                        && a.ParentFolderId == parentFolderId
                        && (a.LeafName.Contains(keyword) || (a.Extension != null && a.Extension.Contains(keyword)));
                }
                else
                {
                    where = a => a.IsDel == (int)FileStatsType.NoDel && a.ParentFolderId == parentFolderId;
                }
            }
            else
            {
                var fileType = (int)request.FileType;
                var keyword = request.KeyWord;

                if (request.FileType != FileTypeEnum.All && !string.IsNullOrWhiteSpace(keyword))
                {
                    where = a => a.IsDel == (int)FileStatsType.NoDel
                        && a.FileType == fileType
                        && a.LeafName.Contains(keyword);
                }
                else if (request.FileType != FileTypeEnum.All)
                {
                    where = a => a.IsDel == (int)FileStatsType.NoDel
                        && a.FileType == fileType;
                }
                else if (!string.IsNullOrWhiteSpace(keyword))
                {
                    // 优化搜索：支持文件名和扩展名搜索，全局搜索
                    where = a => a.IsDel == (int)FileStatsType.NoDel
                        && (a.LeafName.Contains(keyword) || (a.Extension != null && a.Extension.Contains(keyword)));
                }
                else
                {
                    where = a => a.IsDel == (int)FileStatsType.NoDel;
                }
            }

            // 使用分页查询
            var orderBys = new Dictionary<string, bool>
            {
                { nameof(FileEntity.IsFolder), false }, // 文件夹优先（降序）
                { nameof(FileEntity.UpdatedDate), false } // 按修改时间降序
            };

            var (items, total) = await _fileRepository.GetPagingAsync(
                request.PageIndex,
                request.PageSize,
                where,
                orderBys,
                cancellationToken
            );

            // 映射 DTO
            var itemResponseDtos = _mapper.Map<List<FileEntity>, List<ItemResponseDto>>(items);

            // 格式化文件大小
            foreach (var item in itemResponseDtos)
            {
                if (!item.IsFolder && item.FileSize.HasValue)
                {
                    item.FileSizeStr = FileExtensionUtil.GetFileSize(item.FileSize.Value);
                }
                else if (item.IsFolder)
                {
                    item.FileSizeStr = "-";
                }
            }

            response.Data = itemResponseDtos;
            response.TotalCount = total;

            return response;
        }
    }
}
