using System.Net;
using System.Text.Json;
using Ptcent.Cloud.Drive.Application.Contracts.Responses;

namespace Ptcent.Cloud.Drive.Web.Middleware
{
    /// <summary>
    /// 全局异常处理中间件
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var response = new ResponseMessageDto<object>
            {
                IsSuccess = false,
                Message = "系统异常，请稍后重试"
            };

            switch (exception)
            {
                case FluentValidation.ValidationException validationEx:
                    response.Code = WebApiResultCode.ValidationError;
                    response.Message = validationEx.Errors.FirstOrDefault()?.ErrorMessage;
                    _logger.LogWarning(validationEx, "验证失败：{Message}", response.Message);
                    break;

                case UnauthorizedAccessException unauthorizedEx:
                    response.Code = WebApiResultCode.Unauthorized;
                    response.Message = "未授权访问";
                    _logger.LogWarning(unauthorizedEx, "未授权访问");
                    break;

                case KeyNotFoundException notFoundEx:
                    response.Code = WebApiResultCode.NotFound;
                    response.Message = "资源不存在";
                    _logger.LogWarning(notFoundEx, "资源未找到：{Message}", notFoundEx.Message);
                    break;

                default:
                    response.Code = WebApiResultCode.SystemError;
                    _logger.LogError(exception, "系统异常：{Message}", exception.Message);
                    break;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = response.Code;

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
        }
    }

    /// <summary>
    /// 异常处理中间件扩展
    /// </summary>
    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}
