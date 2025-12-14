using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared.Common.Persistence;
using Shared.Infrastructure.Middleware;

namespace Shared.Infrastructure.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseSwaggerConfiguration(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "TransferHub API v1");
            options.RoutePrefix = "swagger";
            options.DisplayRequestDuration();
            options.EnableDeepLinking();
            options.EnableFilter();
            options.EnablePersistAuthorization();
        });

        return app;
    }

    public static IApplicationBuilder UseCommonMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<RequestLoggingMiddleware>();
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        return app;
    }

    public static IApplicationBuilder UseIdempotency(this IApplicationBuilder app)
    {
        app.UseMiddleware<IdempotencyMiddleware>();
        return app;
    }

    public static WebApplication UseHealthChecksConfiguration(this WebApplication app, string path = "/health")
    {
        app.MapHealthChecks(path);

        return app;
    }

    public static async Task<IApplicationBuilder> EnsureDatabaseCreatedAsync<TDbContext>(
        this IApplicationBuilder app)
        where TDbContext : DbContext
    {
        var environment = app.ApplicationServices.GetRequiredService<IHostEnvironment>();

        if (environment.IsDevelopment())
        {
            using var scope = app.ApplicationServices.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
            await dbContext.Database.EnsureCreatedAsync();
        }

        return app;
    }

    public static async Task<IApplicationBuilder> InitializeDatabaseAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();

        var initializers = scope.ServiceProvider.GetServices<IDbInitializer>();

        foreach (var initializer in initializers)
        {
            await initializer.MigrateAsync(CancellationToken.None);
            await initializer.SeedAsync(CancellationToken.None);
        }

        return app;
    }
}