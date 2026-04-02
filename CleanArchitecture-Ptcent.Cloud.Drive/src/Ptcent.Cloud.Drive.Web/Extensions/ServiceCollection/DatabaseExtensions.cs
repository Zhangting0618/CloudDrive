using Microsoft.EntityFrameworkCore;
using Ptcent.Cloud.Drive.Application.Interfaces;
using Ptcent.Cloud.Drive.Infrastructure.Persistence;
using Ptcent.Cloud.Drive.Infrastructure.Persistence.Interceptors;

namespace Ptcent.Cloud.Drive.Web.Extensions.ServiceCollection
{
    /// <summary>
    /// 数据库服务扩展
    /// </summary>
    public static class DatabaseExtensions
    {
        public static IServiceCollection AddDatabase(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("数据库连接未配置");

            // 注册拦截器
            var independentSubmitInterceptor = new IndependentSubmitInterceptor();
            services.AddSingleton(independentSubmitInterceptor);

            services.AddDbContext<AppDbContext>((sp, options) =>
            {
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), mysqlOptions =>
                {
                    mysqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null
                    );
                    mysqlOptions.CommandTimeout(30);
                });

                // 启用懒加载
                options.UseLazyLoadingProxies();

                // 添加拦截器
                options.AddInterceptors(sp.GetRequiredService<IndependentSubmitInterceptor>());
            });

            // 注册工作单元
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
