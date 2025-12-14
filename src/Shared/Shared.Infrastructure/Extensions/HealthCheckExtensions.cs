using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Common.Authentication;

namespace Shared.Infrastructure.Extensions;

public static class HealthCheckExtensions
{
    public static IHealthChecksBuilder AddCommonHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgresDb")
                               ?? throw new ArgumentNullException("Connection strings must be present in the config file");

        var healthChecksBuilder = services.AddHealthChecks()
            .AddNpgSql(connectionString);

        var redisOptions = configuration.GetSection(nameof(RedisOptions)).Get<RedisOptions>();
        if (redisOptions != null && !string.IsNullOrEmpty(redisOptions.ConnectionString))
        {
            healthChecksBuilder.AddRedis(redisOptions.ConnectionString);
        }

        return healthChecksBuilder;
    }
}