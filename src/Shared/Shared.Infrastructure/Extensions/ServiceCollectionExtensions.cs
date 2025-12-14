using System.Reflection;
using CorrelationId.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Shared.Common.Authentication;
using Shared.Infrastructure.Idempotency;

namespace Shared.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiVersioningConfiguration(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
        });

        services.AddVersionedApiExplorer(setup =>
        {
            setup.GroupNameFormat = "'v'VVV";
            setup.SubstituteApiVersionInUrl = true;
        });

        return services;
    }

    public static IServiceCollection AddCorrelationId(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddDefaultCorrelationId(options =>
        {
            options.AddToLoggingScope = true;
            options.RequestHeader = "X-Correlation-ID";
            options.ResponseHeader = "X-Correlation-ID";
            options.IncludeInResponse = true;
        });

        return services;
    }

    public static IServiceCollection AddCommonOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>()
            .BindConfiguration(nameof(JwtOptions));

        services.AddOptions<RedisOptions>()
            .BindConfiguration(nameof(RedisOptions));

        services.AddOptions<RabbitMQOptions>()
            .BindConfiguration(nameof(RabbitMQOptions));

        return services;
    }

    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services, string? assemblyName = null)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "TransferHub API",
                Description = "Money Transfer System API",
                Contact = new OpenApiContact
                {
                    Name = "TransferHub",
                    Email = "support@transferhub.com"
                }
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header
            });

            options.OperationFilter<IdempotencyHeaderOperationFilter>();

            var xmlCommentsPath = GetXmlCommentsPath(assemblyName);

            if (File.Exists(xmlCommentsPath))
                options.IncludeXmlComments(xmlCommentsPath);

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header
                    },
                    new List<string>()
                }
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";

            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }
        });

        return services;
    }

    private static string GetXmlCommentsPath(string? assemblyName = null)
    {
        assemblyName ??= Assembly.GetExecutingAssembly().GetName().Name;
        return Path.Combine(AppContext.BaseDirectory, $"{assemblyName}.xml");
    }
}