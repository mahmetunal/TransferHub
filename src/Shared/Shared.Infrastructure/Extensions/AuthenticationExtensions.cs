using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Shared.Common.Authentication;
using Shared.Common.Identity;
using Shared.Infrastructure.Authentication;

namespace Shared.Infrastructure.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<JwtBearerOptions>? configureJwtBearer = null,
        string configurationSectionName = "JwtOptions")
    {
        JwtOptions jwtOptions;

        if (configurationSectionName == "Jwt")
        {
            jwtOptions = new JwtOptions
            {
                SecretKey = configuration["Jwt:SecretKey"]!,
                Issuer = configuration["Jwt:Issuer"]!,
                Audience = configuration["Jwt:Audience"]!,
                ExpirationMinutes = int.Parse(configuration["Jwt:ExpirationMinutes"] ?? "60")
            };
        }
        else
        {
            jwtOptions = configuration.GetSection(configurationSectionName).Get<JwtOptions>()
                         ?? throw new InvalidOperationException($"Jwt options must be present in the config file under section '{configurationSectionName}'");
        }

        var jwtSecretKey = jwtOptions.SecretKey;
        var jwtIssuer = jwtOptions.Issuer;
        var jwtAudience = jwtOptions.Audience;
        var jwtExpirationMinutes = jwtOptions.ExpirationMinutes;

        var jwtConfiguration = new JwtOptions
        {
            SecretKey = jwtSecretKey,
            Issuer = jwtIssuer,
            Audience = jwtAudience,
            ExpirationMinutes = jwtExpirationMinutes
        };

        services.AddSingleton(jwtConfiguration);
        services.AddSingleton<JwtService>();

        services.AddScoped<ICurrentUser, CurrentUser>();

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers["Token-Expired"] = "true";
                        }

                        return Task.CompletedTask;
                    }
                };

                configureJwtBearer?.Invoke(options);
            });

        services.AddAuthorization();

        return services;
    }
}