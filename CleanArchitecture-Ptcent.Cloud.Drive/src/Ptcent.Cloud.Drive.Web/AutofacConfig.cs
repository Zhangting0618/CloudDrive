using Ptcent.Cloud.Drive.Infrastructure.Persistence;
using System.Reflection;
using Autofac;

namespace Ptcent.Cloud.Drive.Web
{
    /// <summary>
    /// 依赖注入
    /// </summary>
    public class AutofacConfig : global::Autofac.Module
    {
        protected override void Load(global::Autofac.ContainerBuilder builder)
        {
            var infrastructure = Assembly.Load("Ptcent.Cloud.Drive.Infrastructure");

            builder.RegisterAssemblyTypes(infrastructure)
                   .Where(t => t.Name.EndsWith("Repos") ||
                               t.Name.EndsWith("Repository"))
                   .AsImplementedInterfaces()
                   .InstancePerLifetimeScope();
            builder.RegisterType<AppDbContext>()
                   .AsSelf()
                   .InstancePerLifetimeScope();
        }
    }

}
