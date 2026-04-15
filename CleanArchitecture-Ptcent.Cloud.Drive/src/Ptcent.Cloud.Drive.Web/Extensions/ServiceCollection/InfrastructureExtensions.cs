using Ptcent.Cloud.Drive.Application.Interfaces;
using Ptcent.Cloud.Drive.Application.Options;
using Ptcent.Cloud.Drive.Application.Services;
using Ptcent.Cloud.Drive.Application.Interfaces.Persistence;
using Ptcent.Cloud.Drive.Infrastructure.Repositories;
using Ptcent.Cloud.Drive.Infrastructure.Services;
using StackExchange.Redis;

namespace Ptcent.Cloud.Drive.Web.Extensions.ServiceCollection
{
    /// <summary>
    /// Infrastructure 层服务注册扩展
    /// </summary>
    public static class InfrastructureExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // 注册仓储
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IFileRepository, FileRepository>();
            services.AddScoped<IShareRepository, ShareRepository>();
            services.AddScoped<ICollectionRepository, CollectionRepository>();
            services.AddScoped<IOperationLogRepository, OperationLogRepository>();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.Configure<RecycleBinCleanupOptions>(configuration.GetSection("RecycleBinCleanup"));

            // 注册缓存服务
            var redisConfig = configuration.GetSection("Redis").Get<RedisOptions>()
                ?? new RedisOptions { ConnectionString = configuration.GetConnectionString("Redis") };

            if (!string.IsNullOrEmpty(redisConfig.ConnectionString))
            {
                services.AddSingleton<IConnectionMultiplexer>(sp =>
                {
                    var cfg = configuration.GetConnectionString("Redis") ?? "localhost:6379";
                    return ConnectionMultiplexer.Connect(cfg);
                });
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConfig.ConnectionString;
                });
            }
            else
            {
                // 如果没有 Redis，使用内存缓存
                services.AddDistributedMemoryCache();
            }

            // 注册应用服务
            services.AddScoped<ICacheService, CacheService>();
            services.AddSingleton<IIdGeneratorService, IdGeneratorService>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IFileHashService, FileHashService>();
            services.AddScoped<IRecycleBinCleanupService, RecycleBinCleanupService>();
            services.AddScoped<IStorageStatsService, StorageStatsService>();

            return services;
        }
    }

    public class RedisOptions
    {
        public string? ConnectionString { get; set; }
    }
}
