using Autofac;
using Ptcent.Cloud.Drive.Infrastructure.Context;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;

namespace Ptcent.Cloud.Drive.Web
{
    /// <summary>
    /// 依赖注入
    /// </summary>
    public class AutofacConfig : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var infrastructure = Assembly.Load("Ptcent.Cloud.Drive.Infrastructure");

            builder.RegisterAssemblyTypes(infrastructure)
                   .Where(t => t.Name.EndsWith("Repos") ||
                               t.Name.EndsWith("Repository"))
                   .AsImplementedInterfaces()
                   .InstancePerLifetimeScope();
            builder.RegisterType<EFDbContext>()
                   .AsSelf()
                   .InstancePerLifetimeScope();
        }
    }

}
