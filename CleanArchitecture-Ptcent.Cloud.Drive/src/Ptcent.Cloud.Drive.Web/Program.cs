using Autofac;
using Autofac.Extensions.DependencyInjection;
using Ptcent.Cloud.Drive.Application.ResponseMessageUntil;
using Ptcent.Cloud.Drive.Web;
using Ptcent.Cloud.Drive.Web.Extensions.ServiceCollection;
using Ptcent.Cloud.Drive.Web.Filter;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile(
    "Configs/appsettings.json",
    optional: true,
    reloadOnChange: true);

// ================= Logging =================
builder.Logging.ClearProviders(); // 很重要
builder.Logging.AddLog4Net("Configs/log4net.config");


// ================= 基础 =================
builder.Services.AddEncodingSupport();
builder.Services.AddSnowId(builder.Configuration);

// ================= Web =================
builder.Services.AddWebApi();
builder.Services.AddSwaggerSupport();
builder.Services.AddCompressionSupport();
builder.Services.AddCorsSupport();
builder.Services.AddJwtAuth(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IRequestContext, RequestContext>();
builder.Services.AddScoped<IResponseMessageUtil, ResponseMessageUtil>();


// ================= Infrastructure =================
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddMediatRSupport();
builder.Services.AddPluginSystem();
//builder.Services.AddRateLimitSupport(builder.Configuration);   临时注释 报错 还没解决


// ================= Autofac =================
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(c =>
{
    c.RegisterModule(new AutofacConfig());
});

var app = builder.Build();

// ================= Pipeline =================
app.UseSwaggerPipeline();
app.UseCors("AllowCors");
app.UseAuthentication();
app.UseAuthorization();
//app.UseRateLimit();   临时注释 报错 还没解决
app.UseResponseCompression();
app.MapControllers();

app.Run();
