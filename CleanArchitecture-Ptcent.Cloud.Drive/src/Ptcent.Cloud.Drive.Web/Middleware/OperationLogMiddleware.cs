using Ptcent.Cloud.Drive.Application.Interfaces.Persistence;
using Ptcent.Cloud.Drive.Application.Services;
using Ptcent.Cloud.Drive.Domain.Entities;
using System.Diagnostics;
using System.Security.Claims;

namespace Ptcent.Cloud.Drive.Web.Middleware
{
    /// <summary>
    /// 操作日志中间件
    /// </summary>
    public class OperationLogMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OperationLogMiddleware> _logger;

        public OperationLogMiddleware(
            RequestDelegate next,
            IServiceScopeFactory scopeFactory,
            ILogger<OperationLogMiddleware> logger)
        {
            _next = next;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!ShouldLog(context.Request))
            {
                await _next(context);
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            Exception? exception = null;

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                stopwatch.Stop();
                await SaveLogAsync(context, stopwatch.ElapsedMilliseconds, exception);
            }
        }

        private async Task SaveLogAsync(HttpContext context, long durationMs, Exception? exception)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var idGenerator = scope.ServiceProvider.GetRequiredService<IIdGeneratorService>();
                var operationLogRepository = scope.ServiceProvider.GetRequiredService<IOperationLogRepository>();
                var endpoint = context.GetEndpoint();

                long? userId = null;
                var userIdValue = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (long.TryParse(userIdValue, out var parsedUserId))
                {
                    userId = parsedUserId;
                }

                var operationLog = new OperationLogEntity
                {
                    Id = idGenerator.NewId(),
                    UserId = userId,
                    UserName = context.User.FindFirstValue(ClaimTypes.Name),
                    Phone = context.User.FindFirst("Phone")?.Value,
                    Method = context.Request.Method,
                    Path = context.Request.Path.Value ?? string.Empty,
                    ActionName = endpoint?.DisplayName,
                    IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = context.Request.Headers.UserAgent.ToString(),
                    RequestQuery = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : null,
                    StatusCode = context.Response.StatusCode,
                    IsSuccess = exception == null && context.Response.StatusCode < StatusCodes.Status400BadRequest,
                    DurationMs = durationMs,
                    ErrorMessage = exception?.Message,
                    CreatedTime = DateTime.Now
                };

                await operationLogRepository.AddAsync(operationLog);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to persist operation log. Path={Path}", context.Request.Path);
            }
        }

        private static bool ShouldLog(HttpRequest request)
        {
            if (HttpMethods.IsOptions(request.Method) || HttpMethods.IsHead(request.Method))
            {
                return false;
            }

            var path = request.Path.Value?.ToLowerInvariant() ?? string.Empty;
            if (path.StartsWith("/health") ||
                path.StartsWith("/swagger") ||
                path.StartsWith("/api/system/operation-logs"))
            {
                return false;
            }

            if (HttpMethods.IsGet(request.Method))
            {
                return path.Contains("/download");
            }

            return true;
        }
    }
}
