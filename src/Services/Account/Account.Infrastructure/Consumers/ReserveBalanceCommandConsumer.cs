using Account.Application.Commands.ReserveBalance;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using SagaCommands = Shared.Common.Sagas.Commands;
using SagaEvents = Shared.Common.Sagas.Events;

namespace Account.Infrastructure.Consumers;

public sealed class ReserveBalanceCommandConsumer : IConsumer<SagaCommands.ReserveBalanceCommand>
{
    private readonly IMediator _mediator;
    private readonly ILogger<ReserveBalanceCommandConsumer> _logger;

    public ReserveBalanceCommandConsumer(
        IMediator mediator,
        ILogger<ReserveBalanceCommandConsumer> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Consume(ConsumeContext<SagaCommands.ReserveBalanceCommand> context)
    {
        _logger.LogInformation(
            "Received ReserveBalanceCommand for Transfer {TransferId}, Account {AccountIban}, InitiatedBy {InitiatedBy}",
            context.Message.TransferId,
            context.Message.AccountIban,
            context.Message.InitiatedBy);

        var command = new ReserveBalanceCommand
        {
            TransferId = context.Message.TransferId,
            AccountIban = context.Message.AccountIban,
            Amount = context.Message.Amount,
            Currency = context.Message.Currency,
            InitiatedBy = context.Message.InitiatedBy
        };

        var result = await _mediator.Send(command, context.CancellationToken);

        if (!result.Success)
        {
            var balanceReservationFailedEvent = new SagaEvents.BalanceReservationFailedEvent
            {
                TransferId = context.Message.TransferId,
                Reason = result.FailureReason ?? "Unknown error"
            };

            await context.Publish(balanceReservationFailedEvent, context.CancellationToken);

            _logger.LogWarning(
                "Balance reservation failed for Transfer {TransferId}, Reason: {Reason}",
                context.Message.TransferId,
                result.FailureReason);

            return;
        }

        var balanceReservedEvent = new SagaEvents.BalanceReservedEvent
        {
            TransferId = context.Message.TransferId,
            ReservationId = result.ReservationId
        };

        await context.Publish(balanceReservedEvent, context.CancellationToken);

        _logger.LogInformation(
            "Balance reserved successfully for Transfer {TransferId}, ReservationId {ReservationId}",
            context.Message.TransferId,
            result.ReservationId);
    }
}