using FluentValidation;
using MediatR;
using Ptcent.Cloud.Drive.Application.Events;
using Ptcent.Cloud.Drive.Application.PipeLineBehavior;

namespace Ptcent.Cloud.Drive.Web.Extensions.ServiceCollection
{
    /// <summary>
    /// 添加 MediatR
    /// </summary>
    public static class MediatRExtensions
    {
        /// <summary>
        /// 添加 MediatR
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddMediatRSupport(this IServiceCollection services)
        {
            var assembly = typeof(EntityChangedNotification).Assembly;

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
            services.AddValidatorsFromAssembly(assembly);
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            return services;
        }
    }

}
