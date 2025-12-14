using Account.Application.Commands.CreditAccount;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using SagaCommands = Shared.Common.Sagas.Commands;
using SagaEvents = Shared.Common.Sagas.Events;

namespace Account.Infrastructure.Consumers;

public sealed class CreditAccountCommandConsumer : IConsumer<SagaCommands.CreditAccountCommand>
{
    private readonly IMediator _mediator;
    private readonly ILogger<CreditAccountCommandConsumer> _logger;

    public CreditAccountCommandConsumer(
        IMediator mediator,
        ILogger<CreditAccountCommandConsumer> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Consume(ConsumeContext<SagaCommands.CreditAccountCommand> context)
    {
        _logger.LogInformation("Received CreditAccountCommand for Transfer {TransferId}", context.Message.TransferId);

        var command = new CreditAccountCommand
        {
            TransferId = context.Message.TransferId,
            AccountIban = context.Message.AccountIban,
            Amount = context.Message.Amount,
            Currency = context.Message.Currency
        };

        var result = await _mediator.Send(command, context.CancellationToken);

        if (!result.Success)
        {
            var creditFailedEvent = new SagaEvents.CreditFailedEvent
            {
                TransferId = context.Message.TransferId,
                Reason = result.FailureReason ?? "Unknown error"
            };

            await context.Publish(creditFailedEvent, context.CancellationToken);

            _logger.LogWarning("Account credit failed. Published CreditFailedEvent for Transfer {TransferId}, Reason: {Reason}", context.Message.TransferId, result.FailureReason);

            return;
        }

        var destinationCreditedEvent = new SagaEvents.DestinationCreditedEvent
        {
            TransferId = context.Message.TransferId
        };

        await context.Publish(destinationCreditedEvent, context.CancellationToken);

        _logger.LogInformation("Account credited successfully. Published DestinationCreditedEvent for Transfer {TransferId}", context.Message.TransferId);
    }
}