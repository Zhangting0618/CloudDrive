using AutoMapper;
using Ptcent.Cloud.Drive.Application.Dto.RequestModels;
using Ptcent.Cloud.Drive.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ptcent.Cloud.Drive.Application.MappingProfiles.Mapping
{
    public class UserProfile : Profile
    {
       public UserProfile()
        {
            CreateMap<RegistrationAccountRequestDto, UserEntity>();
        }
    }
}
