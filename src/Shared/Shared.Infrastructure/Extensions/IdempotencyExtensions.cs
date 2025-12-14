using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Common.Idempotency;
using Shared.Infrastructure.Idempotency;

namespace Shared.Infrastructure.Extensions;

public static class IdempotencyExtensions
{
    public static IServiceCollection AddIdempotency(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var redisConnectionString = configuration["RedisOptions:ConnectionString"];

        if (string.IsNullOrWhiteSpace(redisConnectionString))
        {
            redisConnectionString = "localhost:6379";
        }

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "TransferHub:";
        });

        services.AddSingleton<IIdempotencyService, RedisIdempotencyService>();

        return services;
    }
}