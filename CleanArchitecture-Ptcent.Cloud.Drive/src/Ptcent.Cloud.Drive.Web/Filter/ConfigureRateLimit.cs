using AspNetCoreRateLimit;
using AspNetCoreRateLimit.Redis;
using StackExchange.Redis;

namespace Ptcent.Cloud.Drive.Web.Filter
{
    public static class ConfigureRateLimit
    {
        public static void AddRateLimit(this IServiceCollection services, IConfiguration conf)
        {
            services.Configure<IpRateLimitOptions>(conf.GetSection("IpRateLimiting"));
            // 注册 Redis 连接服务
            var redisOptions = ConfigurationOptions.Parse(conf["Redis:RedisHost"] + ":" +conf["Redis:RedisPort"]  + ",password=" + conf["Redis:RedisPassword"] +",connectTimeout=2000");
            services.AddSingleton<IConnectionMultiplexer>(provider =>
            {
                return ConnectionMultiplexer.Connect(redisOptions);
            });
            services.AddRedisRateLimiting();
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        }
        public static IApplicationBuilder UseRateLimit(this IApplicationBuilder app)
        {
            app.UseIpRateLimiting();
            return app;
        }
    }
}
