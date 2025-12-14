using System.Reflection;
using CorrelationId;
using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MoneyTransfer.API.Hubs;
using MoneyTransfer.API.Services;
using MoneyTransfer.Application;
using MoneyTransfer.Application.Repositories;
using MoneyTransfer.Application.Sagas;
using MoneyTransfer.Application.Services;
using MoneyTransfer.Infrastructure.Consumers;
using MoneyTransfer.Infrastructure.Persistence;
using MoneyTransfer.Infrastructure.Persistence.Repositories;
using Serilog;
using Shared.Common.Authentication;
using Shared.Common.Messaging;
using Shared.Common.Persistence;
using Shared.Common.Sagas.Events;
using Shared.Infrastructure.Extensions;
using Shared.Infrastructure.Logging;
using Shared.Infrastructure.Persistence;
using Shared.Infrastructure.Persistence.Interceptors;
using DomainEventPublishingInterceptor = Shared.Infrastructure.Persistence.Interceptors.DomainEventPublishingInterceptor;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, _, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.With<HashedLoggingEnricher>()
    .Enrich.WithProperty("Application", "MoneyTransfer.API")
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithCorrelationId());

builder.Services.AddControllers();
builder.Services.AddSwaggerConfiguration(Assembly.GetExecutingAssembly().GetName().Name);
builder.Services.AddApiVersioningConfiguration();

builder.Services.AddCorrelationId();

builder.Services.AddSignalR();
builder.Services.AddCommonOptions(builder.Configuration);

var connectionString = builder.Configuration.GetConnectionString("PostgresDb")
                       ?? throw new ArgumentNullException("Connection strings must be present in the config file");

builder.Services.AddScoped<AuditInterceptor>();
builder.Services.AddScoped<DomainEventPublishingInterceptor>();

builder.Services.AddDbContext<MoneyTransferDbContext>((sp, options) =>
{
    options.UseNpgsql(connectionString, e => e.MigrationsAssembly(typeof(MoneyTransferDbContext).Assembly.GetName().Name))
        .EnableSensitiveDataLogging();

    options.AddInterceptors(sp.GetRequiredService<DomainEventPublishingInterceptor>(),
        sp.GetRequiredService<AuditInterceptor>());
});

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<BalanceReservedEventConsumer>();
    x.AddConsumer<BalanceReservationFailedEventConsumer>();
    x.AddConsumer<TransferCompletedEventConsumer>();
    x.AddConsumer<TransferCancelledEventConsumer>();

    x.AddSagaStateMachine<TransferSagaStateMachine, TransferSagaState>()
        .EntityFrameworkRepository(r =>
        {
            r.ConcurrencyMode = ConcurrencyMode.Optimistic;
            r.ExistingDbContext<MoneyTransferDbContext>();
            r.UsePostgres();
        });

    x.AddEntityFrameworkOutbox<MoneyTransferDbContext>(cfg =>
    {
        cfg.UsePostgres();
        cfg.UseBusOutbox();
    });

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqOptions = context.GetRequiredService<IOptions<RabbitMQOptions>>().Value;

        cfg.Host(rabbitMqOptions.Host, "/", h =>
        {
            h.Username(rabbitMqOptions.Username);
            h.Password(rabbitMqOptions.Password);
        });

        cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));

        cfg.ConfigureQueueDeduplicationEndpoint<BalanceReservedEventConsumer, BalanceReservedEvent>(context);
        cfg.ConfigureQueueDeduplicationEndpoint<BalanceReservationFailedEventConsumer, BalanceReservationFailedEvent>(context);
        cfg.ConfigureQueueDeduplicationEndpoint<TransferCompletedEventConsumer, TransferCompletedEvent>(context);
        cfg.ConfigureQueueDeduplicationEndpoint<TransferCancelledEventConsumer, TransferCancelledEvent>(context);

        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddScoped<ITransferRepository, TransferRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork<MoneyTransferDbContext>>();
builder.Services.AddScoped<IDbInitializer, MoneyTransferDbInitializer>();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    cfg.RegisterServicesFromAssembly(typeof(MoneyTransferMetadata).Assembly);
});

builder.Services.AddValidatorsFromAssembly(typeof(MoneyTransferMetadata).Assembly);

var redisOptions = builder.Configuration.GetSection(nameof(RedisOptions)).Get<RedisOptions>()
                   ?? throw new InvalidOperationException("RedisOptions must be present in the config file");

builder.Services.AddStackExchangeRedisCache(options => { options.Configuration = redisOptions?.ConnectionString; });

builder.Services.AddScoped<ITransferNotificationService, TransferNotificationService>();
builder.Services.AddScoped<IDbInitializer, MoneyTransferDbInitializer>();

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddCommonHealthChecks(builder.Configuration);

builder.Services.AddIdempotency(builder.Configuration);

builder.Services.AddOpenTelemetryObservability(builder.Configuration, "MoneyTransfer.API");

var app = builder.Build();

app.UseCorrelationId();
app.UseSerilogRequestLogging();

app.UseSwaggerConfiguration();
app.UseCommonMiddleware();

app.UseIdempotency();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<TransferHub>("/hubs/transfer");
app.UseHealthChecksConfiguration();

await app.InitializeDatabaseAsync();

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
        logger.LogInformation("MoneyTransfer.API listening on {Address}", address);
    }
});

await app.WaitForShutdownAsync();