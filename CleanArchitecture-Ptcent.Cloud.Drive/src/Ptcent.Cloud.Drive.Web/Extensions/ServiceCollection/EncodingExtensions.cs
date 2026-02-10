using System.Text;

namespace Ptcent.Cloud.Drive.Web.Extensions.ServiceCollection
{
    public static class EncodingExtensions
    {
        public static IServiceCollection AddEncodingSupport(
            this IServiceCollection services)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            return services;
        }
    }

}
