using AspNetCoreRateLimit;
using AspNetCoreRateLimit.Redis;
using StackExchange.Redis;

namespace Ptcent.Cloud.Drive.Web.Filter
{
    /// <summary>
    /// 配置限流
    /// </summary>
    public static class ConfigureRateLimit
    {
        public static IServiceCollection AddRateLimit(
            this IServiceCollection services,
            IConfiguration conf)
        {
            // 1️⃣ 绑定配置
            services.Configure<IpRateLimitOptions>(
                conf.GetSection("IpRateLimiting"));

            services.Configure<IpRateLimitPolicies>(
                conf.GetSection("IpRateLimiting"));

            // 2️⃣ ⭐ 注册 Distributed Redis Cache（关键）
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration =
                    $"{conf["Redis:RedisHost"]}:{conf["Redis:RedisPort"]},password={conf["Redis:RedisPassword"]}";
                options.InstanceName = "IpRateLimit:";
            });

            // 3️⃣ 注册 RateLimit Redis Store
            services.AddRedisRateLimiting();

            // 4️⃣ RateLimit 核心配置
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

            return services;
        }

        public static IApplicationBuilder UseRateLimit(this IApplicationBuilder app)
        {
            app.UseIpRateLimiting();
            return app;
        }
    }

}
