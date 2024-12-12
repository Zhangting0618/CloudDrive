using AutoMapper;
using MediatR;
using Ptcent.Cloud.Drive.Application.Dto.ReponseModels;
using Ptcent.Cloud.Drive.Application.Dto.RequestModels;
using Ptcent.Cloud.Drive.Application.MappingProfiles;
using Ptcent.Cloud.Drive.Domain.Entities;
using Ptcent.Cloud.Drive.Domain.Enum;
using Ptcent.Cloud.Drive.Infrastructure.IRespository;
using Ptcent.Cloud.Drive.Shared.Extensions;
using Ptcent.Cloud.Drive.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ptcent.Cloud.Drive.Application.Handlers.QueryHandlers.File
{
    internal class GetFilesQueryHandler : IRequestHandler<FileRequestDto, ResponseMessageDto<List<ItemResponseDto>>>
    {
        private readonly IFileRepository fileRepository;
        private readonly IMapper mapper;
        public GetFilesQueryHandler(IFileRepository fileRepository, IMapper mapper)
        {
            this.fileRepository = fileRepository;
            this.mapper = mapper;
        }
        public async Task<ResponseMessageDto<List<ItemResponseDto>>> Handle(FileRequestDto request, CancellationToken cancellationToken)
        {
            var response=new ResponseMessageDto<List<ItemResponseDto>>() { IsSuccess=true};
            var fileEntitys=new List<FileEntity>().AsQueryable();
            if (request.FileType== FileTypeEnum.All)
            {
                 fileEntitys = await fileRepository.WhereAsync(a => a.IsDel == 0);
            }
            else
            {
                var fileType = (int)Enum.Parse(typeof(FileTypeEnum), request.FileType.ToString());
                fileEntitys = await fileRepository.WhereAsync(a => a.IsDel == 0&&a.FileType==fileType);
            }
            int count= fileEntitys.Count();
            if (!request.KeyWord.IsNullOrWhiteSpace())
            {
                fileEntitys= fileEntitys.Where(a=>a.LeafName.Contains(request.KeyWord));
            }
            var tempData= fileEntitys.OrderByDescending(a=>a.IsFolder).ThenByDescending(a=>a.UpdatedDate).Skip((request.PageIndex-1)* request.PageSize).Take(request.PageSize).ToList();
            var itemResponseDtos = mapper.Map<List<FileEntity>, List<ItemResponseDto>>(tempData);
            foreach (var itemResponseDto in itemResponseDtos)
            {
               itemResponseDto.FileSizeStr = FileExtensionUtil.GetFileSize(itemResponseDto.FileSize.Value);
            }
            response.Data = itemResponseDtos;
            response.TotalCount = count;
            return response;
        }
    }
}
