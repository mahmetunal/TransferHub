using System.Reflection;
using Account.Application;
using Account.Application.Repositories;
using Account.Infrastructure.Consumers;
using Account.Infrastructure.Persistence;
using Account.Infrastructure.Persistence.Repositories;
using CorrelationId;
using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using Shared.Common.Authentication;
using Shared.Common.Messaging;
using Shared.Common.Persistence;
using Shared.Common.Sagas.Commands;
using Shared.Infrastructure.Extensions;
using Shared.Infrastructure.Logging;
using Shared.Infrastructure.Persistence;
using Shared.Infrastructure.Persistence.Interceptors;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.With<HashedLoggingEnricher>()
    .Enrich.WithProperty("Application", "Account.API")
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithCorrelationId());

builder.Services.AddControllers();
builder.Services.AddSwaggerConfiguration(Assembly.GetExecutingAssembly().GetName().Name);
builder.Services.AddApiVersioningConfiguration();

builder.Services.AddCorrelationId();

builder.Services.AddCommonOptions(builder.Configuration);

var connectionString = builder.Configuration.GetConnectionString("PostgresDb")
                       ?? throw new ArgumentNullException("Connection strings must be present in the config file");

builder.Services.AddScoped<AuditInterceptor>();
builder.Services.AddScoped<DomainEventPublishingInterceptor>();

builder.Services.AddDbContext<AccountDbContext>((sp, options) =>
{
    options.UseNpgsql(connectionString, e => e.MigrationsAssembly(typeof(AccountDbContext).Assembly.GetName().Name))
        .EnableSensitiveDataLogging()
        .EnableDetailedErrors();

    options.AddInterceptors(sp.GetRequiredService<DomainEventPublishingInterceptor>(),
        sp.GetRequiredService<AuditInterceptor>());
});

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ReserveBalanceCommandConsumer>();
    x.AddConsumer<CreditAccountCommandConsumer>();
    x.AddConsumer<CommitReservationCommandConsumer>();
    x.AddConsumer<ReleaseReservationCommandConsumer>();

    x.AddEntityFrameworkOutbox<AccountDbContext>(cfg =>
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

        cfg.ConfigureQueueDeduplicationEndpoint<
            ReserveBalanceCommandConsumer, ReserveBalanceCommand>(context);

        cfg.ConfigureQueueDeduplicationEndpoint<
            CreditAccountCommandConsumer, CreditAccountCommand>(context);

        cfg.ConfigureQueueDeduplicationEndpoint<
            CommitReservationCommandConsumer, CommitReservationCommand>(context);

        cfg.ConfigureQueueDeduplicationEndpoint<
            ReleaseReservationCommandConsumer, ReleaseReservationCommand>(context);

        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IBalanceReservationRepository, BalanceReservationRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork<AccountDbContext>>();
builder.Services.AddScoped<IDbInitializer, AccountDbInitializer>();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    cfg.RegisterServicesFromAssembly(typeof(AccountMetadata).Assembly);
});

builder.Services.AddValidatorsFromAssembly(typeof(AccountMetadata).Assembly);

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddCommonHealthChecks(builder.Configuration);

builder.Services.AddIdempotency(builder.Configuration);

builder.Services.AddOpenTelemetryObservability(
    builder.Configuration, "Account.API");

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
        logger.LogInformation("Account.API listening on {Address}", address);
    }
});

await app.WaitForShutdownAsync();