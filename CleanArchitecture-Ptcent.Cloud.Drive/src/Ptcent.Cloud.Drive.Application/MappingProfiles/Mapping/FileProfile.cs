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
            CreateMap<FileEntity, ItemResponseDto>().ReverseMap();
        }
    }
}
