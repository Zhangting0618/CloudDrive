using MediatR;
using Microsoft.EntityFrameworkCore;
using Ptcent.Cloud.Drive.Infrastructure.Context;
using Ptcent.Cloud.Drive.Infrastructure.Persistence.Interceptors;

namespace Ptcent.Cloud.Drive.Web.Extensions.ServiceCollection
{
    public static class DatabaseExtensions
    {
        public static IServiceCollection AddDatabase(
            this IServiceCollection services,
            IConfiguration configuration)
        {


            var conn = configuration.GetConnectionString(
                "PtcentYiDocUserWebApiConnection")
                ?? throw new InvalidOperationException("数据库连接未配置");

            var version = ServerVersion.AutoDetect(conn);

            services.AddDbContext<EFDbContext>((sp, opt) =>
            {
                opt.UseMySql(conn, version);

                // ⭐ 正确用法
                var mediator = sp.GetRequiredService<IMediator>();
                opt.AddInterceptors(
                    new EntityChangeInterceptor(mediator));

                opt.UseLazyLoadingProxies();
            });

            return services;
        }
    }

}
