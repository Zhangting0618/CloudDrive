using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Ptcent.Cloud.Drive.Web.Filter
{
    /// <summary>
    /// 添加Token 验证
    /// </summary>
    public class AddTokenHeaderParameter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null) operation.Parameters = new List<OpenApiParameter>();
            var descriptor = context.ApiDescription.ActionDescriptor as ControllerActionDescriptor;
            if (descriptor != null)
            {
                operation.Parameters.Add(new OpenApiParameter()
                {
                    Name = "AuthToken",
                    In = ParameterLocation.Header,
                    Required = false,
                    Description = "登录授权码 未登录时传空"
                });

                operation.Parameters.Add(new OpenApiParameter()
                {
                    Name = "Source",
                    In = ParameterLocation.Header,
                    Required = false,
                    Description = "请求类型 PCweb请求请传1"
                });

            }
        }

        public class BinaryPayloadFilter : IOperationFilter
        {
            public void Apply(OpenApiOperation operation, OperationFilterContext context)
            {
                if (context.ApiDescription.HttpMethod.ToUpper() != "POST")
                {
                    return;
                }
                operation.RequestBody = new OpenApiRequestBody();
                operation.RequestBody.Content.Add("application/json", new OpenApiMediaType()
                {
                    Schema = new OpenApiSchema() { Type = "string" }
                });
            }
        }
    }
}
