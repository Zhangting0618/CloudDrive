using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ptcent.Cloud.Drive.Application.Services;
using Ptcent.Cloud.Drive.Application.Interfaces.Persistence;
using Ptcent.Cloud.Drive.Domain.Constants;
using Ptcent.Cloud.Drive.Domain.Enum;
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
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var authorization = context.Request.Headers.Authorization.ToString();
                            if (!string.IsNullOrWhiteSpace(authorization) &&
                                !authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                            {
                                context.Token = authorization;
                            }

                            return Task.CompletedTask;
                        },
                        OnTokenValidated = async context =>
                        {
                            var jti = context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                            if (string.IsNullOrWhiteSpace(jti))
                            {
                                return;
                            }

                            var cacheService = context.HttpContext.RequestServices.GetRequiredService<ICacheService>();
                            var blacklistKey = string.Format(CacheKeys.UserTokenBlacklist, jti);
                            if (await cacheService.ExistsAsync(blacklistKey, context.HttpContext.RequestAborted))
                            {
                                context.Fail("Token has been revoked.");
                                return;
                            }

                            var userRepository = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
                            var userIdValue = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                            if (long.TryParse(userIdValue, out var userId))
                            {
                                var user = await userRepository.GetByIdAsync(userId, context.HttpContext.RequestAborted);
                                if (user == null || user.IsDel == (int)UserStatus.Quit)
                                {
                                    context.Fail("User is disabled.");
                                }
                            }
                        }
                    };

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

            services.AddSingleton<JwtSecurityTokenHandler>();

            return services;
        }
    }
}
