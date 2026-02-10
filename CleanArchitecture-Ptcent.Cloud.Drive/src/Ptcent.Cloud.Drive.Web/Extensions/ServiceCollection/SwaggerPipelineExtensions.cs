namespace Ptcent.Cloud.Drive.Web.Extensions.ServiceCollection
{
    public static class SwaggerPipelineExtensions
    {
        public static IApplicationBuilder UseSwaggerPipeline(
            this IApplicationBuilder app)
        {
            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "CloudDrive API");
                c.RoutePrefix = string.Empty;
            });

            return app;
        }
    }

}
