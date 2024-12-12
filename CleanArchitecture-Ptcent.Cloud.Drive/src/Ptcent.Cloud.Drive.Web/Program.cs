using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Ptcent.Cloud.Drive.Infrastructure.Context;
using Ptcent.Cloud.Drive.Shared.Extensions;
using Ptcent.Cloud.Drive.Shared.Util;
using Ptcent.Cloud.Drive.Web.Filter;
using Ptcent.Cloud.Drive.Web.Options;
using System.Text;
using Ptcent.Cloud.Drive.Application.Dto.Common;
using Ptcent.Cloud.Drive.Shared.RouteUtil;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using MediatR;
using Ptcent.Cloud.Drive.Application.PipeLineBehavior;
using FluentValidation;
using Autofac;
using Ptcent.Cloud.Drive.Web;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Ptcent.Cloud.Drive.Application.MappingProfiles;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;
using Minio;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("Configs/appsettings.json", optional: true, reloadOnChange: true);
//��ֹ��������
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = int.MaxValue;
});
//IIS �ϴ����ֵ
builder.Services.Configure<FormOptions>(x =>
{

    x.ValueLengthLimit = int.MaxValue;
    x.MultipartBodyLengthLimit = int.MaxValue;
});
var assembly = AppDomain.CurrentDomain.Load("Ptcent.Cloud.Drive.Application");
builder.Services.AddMediatR(c =>
{
    c.RegisterServicesFromAssembly(assembly);
});
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddValidatorsFromAssemblies(Enumerable.Repeat(assembly, 1));


var snowIdSection = builder.Configuration.GetSection("SnowId");

#region ѩ��ID
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
#region ѹ����Ӧ����
builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
}).Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
}).AddResponseCompression(options =>
{
    options.EnableForHttps = true;//����Https
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


#region MinIOע��

#endregion

builder.Services.Configure<ApiBehaviorOptions>(opt => opt.SuppressModelStateInvalidFilter = true);
builder.Services.AddControllers(options =>
{
    //ע��ȫ�ֹ�����
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
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "CloudDrive����WebApi", Version = "v1" });
    //options.OperationFilter<BinaryPayloadFilter>();
});

builder.Services.AddSingleton(AutoMapperConfig.GetMapperConfigs());

//����
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
    //ȡ��˽Կ
    var secretByte = Encoding.UTF8.GetBytes(builder.Configuration["Authentication:SecretKey"]!);
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        //��֤������
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Authentication:Issuer"],
        //��֤������
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Authentication:Audience"],
        //��֤�Ƿ����
        ValidateLifetime = true,
        //��֤˽Կ
        IssuerSigningKey = new SymmetricSecurityKey(secretByte)
    };
});

var connection = ConfigUtil.GetValue("PtcentYiDocUserWebApiConnection");
var serverVersion = ServerVersion.AutoDetect(connection);
builder.Services.AddDbContext<EFDbContext>(options => options.UseMySql(connection, serverVersion));// 

HttpContextUtil.serviceCollection = builder.Services;

//���HTTp
//Autozע��
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(builder =>
{
    builder.RegisterModule(new AutofacConfig());
});
builder.Services.AddRateLimit(builder.Configuration);//ע����������
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Logging.AddLog4Net("Configs/log4net.config");

// Add services to the container.

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
#region �� ��
app.UseCors("AllowCors");
//app.Urls.Add("http://localhost:9000");

#endregion
// Configure the HTTP request pipeline.



if (app.Environment.IsDevelopment() || Debugger.IsAttached)
{
    //�����м����������Swagger��ΪJSON�ս��
    app.UseSwagger();
    //�����м�������swagger-ui��ָ��Swagger JSON�ս��
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CloudDrive����WebApi");
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

//��֤
app.UseAuthentication();//��ǰ ��Ȩ
app.UseAuthorization();//�ں�  ��Ȩ
app.UseRateLimit();//�ӿ�ip���� �м��
app.UseResponseCompression();//������Ӧѹ���м��
app.Use((context, next) =>
{
    context.Request.EnableBuffering();
    return next();
});

//��� �ں��
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
LogUtil.Info($"CloudDrive����ʼ����" + DateTime.Now);
app.Run();
