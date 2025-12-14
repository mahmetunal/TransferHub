using Microsoft.Extensions.Configuration;

namespace Shared.Infrastructure.Extensions;

public static class ConfigurationsExtensions
{
    public static IConfigurationBuilder AddLocalJsonConfigurationFiles(this IConfigurationBuilder builder)
    {
        builder.AddJsonFile("appsettings.json", true, true)
            .AddJsonFile("appsettings.Development.json", true, true);

        return builder;
    }
}