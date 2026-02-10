namespace Ptcent.Cloud.Drive.Web.Extensions.ServiceCollection
{
    /// <summary>
    /// 插件系统
    /// </summary>
    public static class PluginSystemExtensions
    {
        /// <summary>
        /// 添加插件系统
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddPluginSystem(
            this IServiceCollection services)
        {
            //services.AddScoped<IPluginDispatcher, PluginDispatcher>();

           // services.AddScoped<IPluginExecutionStore, EfPluginExecutionStore>();

            //services.AddSingleton<IAsyncPluginQueue, InMemoryAsyncPluginQueue>();

           // services.AddHostedService<AsyncPluginWorker>();

            return services;
        }
    }

}
