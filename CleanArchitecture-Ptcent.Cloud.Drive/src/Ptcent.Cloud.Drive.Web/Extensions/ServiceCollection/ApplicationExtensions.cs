using FluentValidation;
using MediatR;
using Ptcent.Cloud.Drive.Application.Features.Users.Commands;
using Ptcent.Cloud.Drive.Application.PipelineBehaviors;

namespace Ptcent.Cloud.Drive.Web.Extensions.ServiceCollection
{
    /// <summary>
    /// Application 层服务注册扩展
    /// </summary>
    public static class ApplicationExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // 注册 MediatR
            var assembly = typeof(RegisterUserCommand).Assembly;
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

            // 注册 FluentValidation
            services.AddValidatorsFromAssembly(assembly);

            // 注册验证行为管道
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            return services;
        }
    }
}
