using AutoMapper;
using Ptcent.Cloud.Drive.Application.Dto.ReponseModels;
using Ptcent.Cloud.Drive.Application.Dto.RequestModels;
using Ptcent.Cloud.Drive.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ptcent.Cloud.Drive.Application.MappingProfiles.Mapping
{
    public class FileProfile : Profile
    {
        public FileProfile()
        {
            CreateMap<FileEntity, ItemResponseDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.LeafName))
                .ForMember(dest => dest.IsFolder, opt => opt.MapFrom(src => src.IsFolder == 1))
                .ForMember(dest => dest.FileType, opt => opt.MapFrom(src => GetFileTypeString(src.FileType)))
                .ForMember(dest => dest.PreviewUrl, opt => opt.MapFrom(src => src.ItemFileMapUrl))
                .ReverseMap();
        }

        private static string? GetFileTypeString(int? fileType)
        {
            return fileType switch
            {
                0 => "unknown",
                1 => "document",
                2 => "image",
                3 => "video",
                4 => "audio",
                5 => "archive",
                _ => "unknown"
            };
        }
    }
}
