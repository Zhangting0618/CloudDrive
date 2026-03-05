namespace Ptcent.Cloud.Drive.Web.Extensions.ServiceCollection
{
    /// <summary>
    /// Web API 服务扩展
    /// </summary>
    public static class WebApiExtensions
    {
        public static IServiceCollection AddWebApiServices(this IServiceCollection services)
        {
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = null;
                    options.JsonSerializerOptions.WriteIndented = false;
                });

            services.AddEndpointsApiExplorer();

            // 添加 HTTP 客户端工厂
            services.AddHttpClient();

            // 添加响应压缩
            services.AddResponseCompression();

            return services;
        }
    }
}
