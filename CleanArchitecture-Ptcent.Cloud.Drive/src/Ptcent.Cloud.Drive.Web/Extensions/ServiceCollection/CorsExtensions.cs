namespace Ptcent.Cloud.Drive.Web.Extensions.ServiceCollection
{
    public static class CorsExtensions
    {
        public static IServiceCollection AddCorsSupport(
            this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowCors", builder =>
                {
                    builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });

            return services;
        }
    }

}
