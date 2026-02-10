using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Ptcent.Cloud.Drive.Web.Filter;

namespace Ptcent.Cloud.Drive.Web.Extensions.ServiceCollection
{
    public static class SwaggerExtensions
    {
        public static IServiceCollection AddSwaggerSupport(
            this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "CloudDrive API",
                    Version = "v1"
                });

                options.CustomOperationIds(api =>
                {
                    var action = api.ActionDescriptor as ControllerActionDescriptor;
                    return $"{action?.ControllerName}-{action?.ActionName}";
                });

                options.OperationFilter<AddTokenHeaderParameter>();

                var basePath = AppContext.BaseDirectory;
                foreach (var file in Directory.GetFiles(basePath, "*.xml"))
                {
                    options.IncludeXmlComments(file, true);
                }
            });

            return services;
        }
    }

}
