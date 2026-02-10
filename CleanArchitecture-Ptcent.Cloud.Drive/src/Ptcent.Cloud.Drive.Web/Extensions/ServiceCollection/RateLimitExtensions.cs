using AspNetCoreRateLimit;
using AspNetCoreRateLimit.Redis;
using Ptcent.Cloud.Drive.Web.Filter;
using StackExchange.Redis;

namespace Ptcent.Cloud.Drive.Web.Extensions.ServiceCollection
{
    public static class RateLimitExtensions
    {
        public static IServiceCollection AddRateLimitSupport(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<IpRateLimitOptions>(
                configuration.GetSection("IpRateLimiting"));

            services.Configure<IpRateLimitPolicies>(
                configuration.GetSection("IpRateLimitPolicies"));

            // Redis 连接
            var redisOptions = ConfigurationOptions.Parse(
                $"{configuration["Redis:RedisHost"]}:{configuration["Redis:RedisPort"]}," +
                $"password={configuration["Redis:RedisPassword"]}," +
                "connectTimeout=2000");

            services.AddSingleton<IConnectionMultiplexer>(
                _ => ConnectionMultiplexer.Connect(redisOptions));

            // ⚠️ Redis 必备组件
          //  services.AddSingleton<IIpPolicyStore, RedisIpPolicyStore>();
          //  services.AddSingleton<IRateLimitCounterStore, RedisRateLimitCounterStore>();

            // ⭐⭐ 核心：Redis 专用 ProcessingStrategy
            services.AddSingleton<IProcessingStrategy, RedisProcessingStrategy>();

            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

            services.AddRedisRateLimiting();

            return services;
        }

        public static IApplicationBuilder UseRateLimitSupport(
            this IApplicationBuilder app)
        {
            app.UseIpRateLimiting();
            return app;
        }
    }
}
