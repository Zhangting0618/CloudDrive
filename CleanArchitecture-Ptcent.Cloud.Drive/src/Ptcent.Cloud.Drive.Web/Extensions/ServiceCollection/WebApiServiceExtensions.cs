using Microsoft.AspNetCore.Mvc;
using Ptcent.Cloud.Drive.Web.Filter;

namespace Ptcent.Cloud.Drive.Web.Extensions.ServiceCollection
{
    public static class WebApiServiceExtensions
    {
        public static IServiceCollection AddWebApi(
            this IServiceCollection services)
        {
            services.Configure<ApiBehaviorOptions>(opt =>
                opt.SuppressModelStateInvalidFilter = true);

            services.AddControllers(opt =>
            {
                opt.Filters.Add<PtcentYiDocApiOperationFilter>();
                opt.Filters.Add<ExceptionFilterAttribute>();
            })
            .AddJsonOptions(o =>
            {
                o.JsonSerializerOptions.PropertyNamingPolicy = null;
            })
            .AddNewtonsoftJson(o =>
            {
                o.SerializerSettings.ReferenceLoopHandling =
                    Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                o.SerializerSettings.DateFormatString =
                    "yyyy-MM-dd HH:mm:ss";
            });

            services.AddHttpContextAccessor();

            return services;
        }
    }

}
