using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;

namespace Ptcent.Cloud.Drive.Web.Extensions.ServiceCollection
{
    public static class CompressionExtensions
    {
        public static IServiceCollection AddCompressionSupport(
            this IServiceCollection services)
        {
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
            });

            services.Configure<BrotliCompressionProviderOptions>(o =>
            {
                o.Level = CompressionLevel.Optimal;
            });

            services.Configure<GzipCompressionProviderOptions>(o =>
            {
                o.Level = CompressionLevel.Optimal;
            });

            return services;
        }
    }

}
