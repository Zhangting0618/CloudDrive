using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Ptcent.Cloud.Drive.Web.Extensions.ServiceCollection
{
    public static class JwtAuthExtensions
    {
        public static IServiceCollection AddJwtAuth(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var secret = configuration["Authentication:SecretKey"]
                ?? throw new InvalidOperationException("JWT SecretKey 未配置");

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidIssuer = configuration["Authentication:Issuer"],

                            ValidateAudience = true,
                            ValidAudience = configuration["Authentication:Audience"],

                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,

                            IssuerSigningKey =
                                new SymmetricSecurityKey(
                                    Encoding.UTF8.GetBytes(secret)),

                            ClockSkew = TimeSpan.Zero
                        };
                });

            // ⚠️ 关键：OperationFilter 依赖它
            services.AddSingleton<JwtSecurityTokenHandler>();

            return services;
        }
    }

}
