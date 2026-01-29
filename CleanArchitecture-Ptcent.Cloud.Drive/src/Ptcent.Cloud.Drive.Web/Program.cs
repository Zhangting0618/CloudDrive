using Autofac;
using Autofac.Extensions.DependencyInjection;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Ptcent.Cloud.Drive.Application.Dto.Common;
using Ptcent.Cloud.Drive.Application.MappingProfiles;
using Ptcent.Cloud.Drive.Application.PipeLineBehavior;
using Ptcent.Cloud.Drive.Infrastructure.Context;
using Ptcent.Cloud.Drive.Infrastructure.Persistence.Interceptors;
using Ptcent.Cloud.Drive.Shared.Extensions;
using Ptcent.Cloud.Drive.Shared.RouteUtil;
using Ptcent.Cloud.Drive.Shared.Util;
using Ptcent.Cloud.Drive.Web;
using Ptcent.Cloud.Drive.Web.Filter;
using Ptcent.Cloud.Drive.Web.Options;
using System.Diagnostics;
using System.IO.Compression;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("Configs/appsettings.json", optional: true, reloadOnChange: true);
//防止中文乱码
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = int.MaxValue;
});
//IIS 上传最大值
builder.Services.Configure<FormOptions>(x =>
{

    x.ValueLengthLimit = int.MaxValue;
    x.MultipartBodyLengthLimit = int.MaxValue;
});



var snowIdSection = builder.Configuration.GetSection("SnowId");

#region 雪花ID
SnowIdOptions snowIdOptions = new SnowIdOptions
{
    Method = snowIdSection.GetValue<short>("Method"),
    BaseTime = DateTime.Parse(snowIdSection["BaseTime"]),
    WorkerId = snowIdSection.GetValue<ushort>("WorkerId"),
    WorkerIdBitLength = (byte)snowIdSection.GetValue<int>("WorkerIdBitLength"),
    SeqBitLength = (byte)snowIdSection.GetValue<int>("SeqBitLength"),
    MaxSeqNumber = snowIdSection.GetValue<int>("MaxSeqNumber"),
    MinSeqNumber = snowIdSection.GetValue<ushort>("MinSeqNumber"),
    TopOverCostCount = snowIdSection.GetValue<int>("TopOverCostCount"),
    DataCenterId = snowIdSection.GetValue<ushort>("DataCenterId"),
    DataCenterIdBitLength = (byte)snowIdSection.GetValue<ushort>("DataCenterIdBitLength"),
    TimestampType = (byte)snowIdSection.GetValue<ushort>("TimestampType")
};
builder.Services.AddIdGenerator(snowIdOptions);
#endregion
#region 压缩响应请求
builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
}).Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
}).AddResponseCompression(options =>
{
    options.EnableForHttps = true;//启用Https
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
       {
            "text/html; charset=utf-8",
            "application/xhtml+xml",
            "application/atom+xml",
            "image/svg+xml"
        });
});
#endregion


#region MinIO注入

#endregion

builder.Services.Configure<ApiBehaviorOptions>(opt => opt.SuppressModelStateInvalidFilter = true);
builder.Services.AddControllers(options =>
{
    //注入全局过滤器
    options.Filters.Add(typeof(PtcentYiDocApiOperationFilter));
    options.Filters.Add(typeof(ExceptionFilterAttribute));
}).AddJsonOptions(config =>
{
    config.JsonSerializerOptions.PropertyNamingPolicy = null;
})
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
    });
builder.Services.AddSwaggerGen(options =>
{

    options.CustomOperationIds(apiDesc =>
    {
        var controllerAction = apiDesc.ActionDescriptor as ControllerActionDescriptor;
        return controllerAction!.ControllerName + "-" + controllerAction.ActionName;
    });
    options.OperationFilter<AddTokenHeaderParameter>();
    var basePath = PlatformServices.Default.Application.ApplicationBasePath;
    DirectoryInfo directory = new(basePath);
    FileInfo[] files = directory.GetFiles("*.xml");
    var xmls = files.Select(a => Path.Combine(basePath, a.FullName)).ToList();
    foreach (var item in xmls)
    {
        options.IncludeXmlComments(item, true);
    }
    options.DocumentFilter<SwaggerEnumFilter>();
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "CloudDrive服务WebApi", Version = "v1" });
    //options.OperationFilter<BinaryPayloadFilter>();
});

builder.Services.AddSingleton(AutoMapperConfig.GetMapperConfigs());

//跨域
builder.Services.AddCors(option => option.AddPolicy("AllowCors", (_builder) =>
{
    _builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
}));
//MemoryCache
builder.Services.AddMemoryCache(option =>
{
    option.Clock = new LocalClockDto();
});
builder.Services.AddHttpClient();

builder.Services.AddMvc(opt =>
{
    opt.UseCentralRoutePrefix(new RouteAttribute("cloudapi/"));
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    //取出私钥
    var secretByte = Encoding.UTF8.GetBytes(builder.Configuration["Authentication:SecretKey"]!);
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        //验证发布者
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Authentication:Issuer"],
        //验证接收者
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Authentication:Audience"],
        //验证是否过期
        ValidateLifetime = true,
        //验证私钥
        IssuerSigningKey = new SymmetricSecurityKey(secretByte)
    };
});

var connection = ConfigUtil.GetValue("PtcentYiDocUserWebApiConnection");
var serverVersion = ServerVersion.AutoDetect(connection);
builder.Services.AddDbContext<EFDbContext>((sp, options) =>
{
    // 数据库配置
    options.UseMySql(connection, serverVersion);

    // ✅ 添加 EntityChangeInterceptor
    var mediator = sp.GetRequiredService<IMediator>();
    options.AddInterceptors(new EntityChangeInterceptor(mediator));

    // 保留 LazyLoading
    options.UseLazyLoadingProxies();

    // 控制台打印 SQL
    if (ConfigUtil.GetValue("IsOpenPrintSQL") == "on")
    {
        options.UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()));
    }
});

var assembly = AppDomain.CurrentDomain.Load("Ptcent.Cloud.Drive.Application");
builder.Services.AddMediatR(c =>
{
    c.RegisterServicesFromAssembly(assembly);
});
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddValidatorsFromAssemblies(Enumerable.Repeat(assembly, 1));

HttpContextUtil.serviceCollection = builder.Services;

//添加HTTp
//Autoz注入
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(builder =>
{
    builder.RegisterModule(new AutofacConfig());
});
builder.Services.AddRateLimit(builder.Configuration);//注册限流服务
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Logging.AddLog4Net("Configs/log4net.config");

// Add services to the container.

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
#region 跨 域
app.UseCors("AllowCors");

#endregion
// Configure the HTTP request pipeline.



if (app.Environment.IsDevelopment() || Debugger.IsAttached)
{
    //启用中间件服务生成Swagger作为JSON终结点
    app.UseSwagger();
    //启用中间件服务对swagger-ui，指定Swagger JSON终结点
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CloudDrive服务WebApi");
        c.RoutePrefix = string.Empty;
    });
}
string fileRootPath = builder.Configuration["FileRootPath"];

string tempUploadFilePath = Path.Combine(fileRootPath, "TempFile");

if (!Directory.Exists(tempUploadFilePath))
{
    Directory.CreateDirectory(tempUploadFilePath);
}

string uploadFilePath = Path.Combine(fileRootPath, "Upload");

if (!Directory.Exists(uploadFilePath))
{
    Directory.CreateDirectory(uploadFilePath);//AppDomain.CurrentDomain.BaseDirectory
}
string sourceFilePath = Path.Combine(fileRootPath, "SourceFiles");

if (!Directory.Exists(sourceFilePath))
{
    Directory.CreateDirectory(sourceFilePath);
}
// Configure the HTTP request pipeline.

//验证
app.UseAuthentication();//在前 鉴权
app.UseAuthorization();//在后  授权
app.UseRateLimit();//接口ip限流 中间件
app.UseResponseCompression();//启用响应压缩中间件
app.Use((context, next) =>
{
    context.Request.EnableBuffering();
    return next();
});

//这个 在后边
app.UseRouting();
app.Use(async (k, next) =>
{
    if (k.Request.Method == "OPTIONS")
    {
        k.Response.StatusCode = 200;
        await k.Response.WriteAsync("ok");
    }
    await next();
});

app.MapControllers();

//app.UseEndpoints(endpoints =>
//{
//    endpoints.MapControllers();
//});
LogUtil.Info($"CloudDrive程序开始启动" + DateTime.Now);
app.Run();
