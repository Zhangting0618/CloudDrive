using Ptcent.Cloud.Drive.Web.Extensions.ServiceCollection;
using Ptcent.Cloud.Drive.Application.MappingProfiles;
using Ptcent.Cloud.Drive.Web.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ================= 配置 =================
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// ================= Logging =================
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// ================= 基础服务 =================
builder.Services.AddSnowId(builder.Configuration);

// ================= Application 层 =================
builder.Services.AddApplicationServices();
builder.Services.AddAutoMapper(typeof(AutoMapperConfig).Assembly);

// ================= Infrastructure 层 =================
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);

// ================= Web API =================
builder.Services.AddWebApi();
builder.Services.AddSwaggerSupport();
builder.Services.AddCorsSupport();
builder.Services.AddJwtAuth(builder.Configuration);

// ================= HTTP 上下文 =================
builder.Services.AddHttpContextAccessor();
builder.Services.AddResponseCompression();

var app = builder.Build();

// ================= Pipeline =================
app.UseExceptionHandling();
app.UseSwaggerPipeline();
app.UseCors("AllowCors");
app.UseAuthentication();
app.UseAuthorization();
app.UseResponseCompression();
app.MapControllers();

// 健康检查端点
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }));

app.Run();
