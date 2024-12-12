using AutoMapper;
using Ptcent.Cloud.Drive.Application.Dto.ReponseModels;
using Ptcent.Cloud.Drive.Application.Dto.RequestModels;
using Ptcent.Cloud.Drive.Application.MappingProfiles.Mapping;
using Ptcent.Cloud.Drive.Domain.Entities;

namespace Ptcent.Cloud.Drive.Application.MappingProfiles
{
    public static class AutoMapperConfig
    {
        /// <summary>
        /// 映射
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        //public static TDestination Map<TSource, TDestination>(TSource source)
        //{
        //    return mapper.Map<TSource, TDestination>(source);
        //}
        public static IMapper GetMapperConfigs()
        {
            var mappingConfig = new MapperConfiguration(mc =>
            {
                // mc.AddProfile(new UserProfile());// 这种写法 也可以
                // mc.AddProfile(new FileProfile());
                mc.CreateMap<FileEntity, ItemResponseDto>().ReverseMap();
            });
            return mappingConfig.CreateMapper();
        }
    }
}
