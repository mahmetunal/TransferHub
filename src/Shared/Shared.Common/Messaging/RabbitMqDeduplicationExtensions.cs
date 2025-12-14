using MassTransit;

namespace Shared.Common.Messaging;

public static class RabbitMqDeduplicationExtensions
{
    private static Task AddDeduplicationHeader<TMessage>(SendContext publishContext)
        where TMessage : class, IDeduplicated
    {
        if (publishContext is SendContext<TMessage> context)
        {
            publishContext.Headers.Set("x-deduplication-header", context.Message.DeduplicationKey);
        }

        return Task.CompletedTask;
    }

    public static void ConfigureQueueDeduplicationEndpoint<TEventConsumer, TMessage>(
        this IRabbitMqBusFactoryConfigurator config,
        IBusRegistrationContext context,
        Action<IRabbitMqReceiveEndpointConfigurator>? configureEndpoint = null)
        where TEventConsumer : class, IConsumer<TMessage>
        where TMessage : class, IDeduplicated
    {
        config.ReceiveEndpoint(typeof(TEventConsumer).Name, endpointConfig =>
        {
            endpointConfig.ConfigureConsumer<TEventConsumer>(context);

            configureEndpoint?.Invoke(endpointConfig);

            endpointConfig.SetQueueArgument("x-message-deduplication", true);
        });

        config.ConfigurePublish(pipeConfigurator => pipeConfigurator.UseExecuteAsync(async ctx => await AddDeduplicationHeader<TMessage>(ctx)));
    }
}