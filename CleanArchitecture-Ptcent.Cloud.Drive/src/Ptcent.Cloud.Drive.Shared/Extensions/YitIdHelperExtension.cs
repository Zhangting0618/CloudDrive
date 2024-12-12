using Microsoft.Extensions.DependencyInjection;
using Yitter.IdGenerator;

namespace Ptcent.Cloud.Drive.Shared.Extensions
{
    public static class YitIdHelperExtension
    {
        /// <summary>
        /// 盐值
        /// </summary>
        private const string Salt = "dkjsDLVK1S7y5be8XYzoq0C2nHaEmBfA";

        /// <summary>
        /// 注册雪花ID服务（默认配置）
        /// </summary>
        /// <param name="service"></param>
        public static void AddIdGenerator(this IServiceCollection service)
        {
            service.AddIdGenerator(new IdGeneratorOptions(0));
        }

        /// <summary>
        /// 注册雪花ID服务（自定义配置）
        /// </summary>
        /// <param name="service"></param>
        /// <param name="options">配置</param>
        public static void AddIdGenerator(this IServiceCollection service, IdGeneratorOptions options)
        {
            options.BaseTime = options.BaseTime.ToUniversalTime();
            YitIdHelper.SetIdGenerator(options);
            service.AddSingleton<IIdGenerator>(new DefaultIdGenerator(options));
        }

        /// <summary>
        /// 生成ID
        /// </summary>
        /// <param name="generator"></param>
        /// <returns></returns>
        public static long NextId(this IIdGenerator generator)
        {
            return generator.NewLong();
        }

    }
}
