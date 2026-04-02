using AutoMapper;
using MediatR;
using Ptcent.Cloud.Drive.Application.Contracts.Responses;
using Ptcent.Cloud.Drive.Application.Dto.ReponseModels;
using Ptcent.Cloud.Drive.Application.Features.Files.Queries;
using Ptcent.Cloud.Drive.Application.Interfaces.Persistence;
using Ptcent.Cloud.Drive.Domain.Entities;
using Ptcent.Cloud.Drive.Domain.Enum;
using Ptcent.Cloud.Drive.Shared.Util;
using System.Linq.Expressions;

namespace Ptcent.Cloud.Drive.Application.Handlers.QueryHandlers.File
{
    /// <summary>
    /// 回收站列表查询处理器
    /// </summary>
    public class RecycleBinQueryHandler : IRequestHandler<RecycleBinQuery, ResponseMessageDto<List<ItemResponseDto>>>
    {
        private readonly IFileRepository _fileRepository;
        private readonly IMapper _mapper;

        public RecycleBinQueryHandler(IFileRepository fileRepository, IMapper mapper)
        {
            _fileRepository = fileRepository;
            _mapper = mapper;
        }

        public async Task<ResponseMessageDto<List<ItemResponseDto>>> Handle(RecycleBinQuery request, CancellationToken cancellationToken)
        {
            var response = new ResponseMessageDto<List<ItemResponseDto>> { IsSuccess = true };

            // 查询已删除的文件（软删除）
            Expression<Func<FileEntity, bool>> where = a =>
                a.IsDel == (int)FileStatsType.Del;

            var orderBys = new Dictionary<string, bool>
            {
                { nameof(FileEntity.DeletedDate), false }, // 按删除时间降序
                { nameof(FileEntity.IsFolder), false },
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
