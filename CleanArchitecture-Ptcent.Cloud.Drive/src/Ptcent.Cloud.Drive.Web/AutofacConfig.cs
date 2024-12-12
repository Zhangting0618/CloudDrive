using Autofac;
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
            var repositories = Assembly.Load("Ptcent.Cloud.Drive.Infrastructure");
            builder.RegisterAssemblyTypes(repositories).Where(t => t.Name.EndsWith("Repos") || t.Name.EndsWith("Repository")).AsImplementedInterfaces();
            builder.RegisterType<JwtSecurityTokenHandler>().InstancePerLifetimeScope();
        }
    }
}
