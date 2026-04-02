using FluentValidation;
using MediatR;
using Ptcent.Cloud.Drive.Application.Attributes;
using Ptcent.Cloud.Drive.Application.Features.Users.Commands;
using Ptcent.Cloud.Drive.Application.PipelineBehaviors;
using Ptcent.Cloud.Drive.Infrastructure.Persistence.Behaviors;
using Ptcent.Cloud.Drive.Infrastructure.Persistence.Interceptors;
using System.Reflection;

namespace Ptcent.Cloud.Drive.Web.Extensions.ServiceCollection
{
    /// <summary>
    /// Application 层服务注册扩展
    /// </summary>
    public static class ApplicationExtensions
    {
        /// <summary>
        /// 事务管理模式
        /// </summary>
        public enum TransactionMode
        {
            /// <summary>
            /// 自动模式：为所有 Command 自动添加事务（推荐）
            /// </summary>
            Automatic,

            /// <summary>
            /// 显式模式：只有标记了 [Transactional] 特性的 Command 才有事务
            /// </summary>
            Explicit
        }

        private static TransactionMode _transactionMode = TransactionMode.Automatic;

        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
            TransactionMode transactionMode = TransactionMode.Automatic)
        {
            _transactionMode = transactionMode;

            // 注册 MediatR
            var assembly = typeof(RegisterUserCommand).Assembly;
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

            // 注册 FluentValidation
            services.AddValidatorsFromAssembly(assembly);

            // 注册验证行为管道
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            // 根据模式注册事务行为管道
            if (_transactionMode == TransactionMode.Automatic)
            {
                // 自动模式：所有 Command 都有事务
                services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AutomaticTransactionBehavior<,>));
            }
            else
            {
                // 显式模式：只有标记 [Transactional] 的 Command 才有事务
                services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
            }

            // 注册独立提交实体类型
            RegisterIndependentEntities();

            return services;
        }

        /// <summary>
        /// 注册独立提交实体类型
        /// 标记了 [IndependentSubmitEntity] 的实体将立即提交，不受事务回滚影响
        /// </summary>
        private static void RegisterIndependentEntities()
        {
            var assembly = typeof(TransactionalAttribute).Assembly;
            var types = assembly.GetTypes()
                .Where(t => t.GetCustomAttribute<IndependentSubmitEntityAttribute>() != null);

            foreach (var type in types)
            {
                IndependentSubmitInterceptor.RegisterIndependentEntityType(type);
            }
        }
    }
}
