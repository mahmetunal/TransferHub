using System.Reflection;
using CorrelationId;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Serilog;
using Shared.Infrastructure.Extensions;
using Shared.Infrastructure.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.With<HashedLoggingEnricher>()
    .Enrich.WithProperty("Application", "BFF.API")
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithCorrelationId());

builder.Services.AddControllers();
builder.Services.AddSwaggerConfiguration(Assembly.GetExecutingAssembly().GetName().Name);

builder.Services.AddApiVersioningConfiguration();

builder.Services.AddCorrelationId();

builder.Services.AddSignalR();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddJwtAuthentication(builder.Configuration, configureJwtBearer: options =>
{
    options.Events.OnMessageReceived = context =>
    {
        var accessToken = context.Request.Query["access_token"];
        var path = context.HttpContext.Request.Path;
        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
        {
            context.Token = accessToken;
        }

        return Task.CompletedTask;
    };
}, configurationSectionName: "JwtOptions");

builder.Services.AddAuthorization(options => { options.AddPolicy("authenticated", policy => { policy.RequireAuthenticatedUser(); }); });

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(builder.Configuration["AllowedOrigins"]?.Split(',')
                           ?? ["http://localhost:3000", "http://localhost:5173"])
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

builder.Services.AddHealthChecks();

builder.Services.AddIdempotency(builder.Configuration);

builder.Services.AddOpenTelemetryObservability(
    builder.Configuration,
    "BFF.API");

var app = builder.Build();

app.UseCorrelationId();
app.UseCommonMiddleware();

app.UseSwaggerConfiguration();

app.UseIdempotency();
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapReverseProxy();
app.UseHealthChecksConfiguration();

await app.StartAsync();

var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
var logger = app.Services.GetRequiredService<ILogger<Program>>();

lifetime.ApplicationStarted.Register(() =>
{
    var addresses = app.Services.GetRequiredService<IServer>()
        .Features.Get<IServerAddressesFeature>()
        ?.Addresses ?? Array.Empty<string>();

    foreach (var address in addresses)
    {
        logger.LogInformation("BFF Gateway listening on {Address}", address);
    }
});

await app.WaitForShutdownAsync();