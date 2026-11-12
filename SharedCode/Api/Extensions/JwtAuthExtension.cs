using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SharedCode.Contracts.Results;
using System.Security.Cryptography;

namespace SharedCode.Api.Extensions
{
    public static class JwtAuthExtensions
    {
        public static IServiceCollection AddJwtAuth(this IServiceCollection services, IConfiguration config)
        {
            var jwt = config.GetSection("Jwt");

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.MapInboundClaims = false; 

                    var parameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwt["Issuer"],
                        ValidateAudience = true,
                        ValidAudience = jwt["Audience"],
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromSeconds(30),
                        ValidAlgorithms = new[] { SecurityAlgorithms.RsaSha256 },
                        NameClaimType = "sub",
                        RoleClaimType = "role"
                    };

                    if (!string.IsNullOrEmpty(jwt["Authority"]))
                    {
                        // Cách B: tự tải JWKS từ AuthService
                        options.Authority = jwt["Authority"];
                        options.RequireHttpsMetadata = false; // chỉ cho dev (http)
                    }
                    else
                    {
                        // Cách A: dùng file public.pem
                        var rsa = RSA.Create();
                        rsa.ImportFromPem(File.ReadAllText(jwt["PublicKeyPath"]!));
                        parameters.IssuerSigningKey = new RsaSecurityKey(rsa);
                        parameters.ValidateIssuerSigningKey = true;
                    }

                    options.TokenValidationParameters = parameters;

                    // Trả JSON thay vì body rỗng, khớp với ErrorResponse của bạn
                    options.Events = new JwtBearerEvents
                    {
                        OnChallenge = async ctx =>
                        {
                            ctx.HandleResponse();
                            ctx.Response.StatusCode = 401;
                            await ctx.Response.WriteAsJsonAsync<Result>(Result.Failure("Unauthorized", ErrorType.Unauthorized));
                        },
                        OnForbidden = async ctx =>
                        {
                            ctx.Response.StatusCode = 403;
                            await ctx.Response.WriteAsJsonAsync<Result>(Result.Failure("Forbidden", ErrorType.Forbidden));
                            //await ctx.Response.WriteAsJsonAsync(new { isSuccess = false, error = "Forbidden", errorType = "Forbidden" });
                        }
                    };
                });

            services.AddAuthorization();
            return services;
        }
    }
}