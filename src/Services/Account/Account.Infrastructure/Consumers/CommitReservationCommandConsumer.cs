using Account.Application.Commands.CommitReservation;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using SagaCommands = Shared.Common.Sagas.Commands;
using SagaEvents = Shared.Common.Sagas.Events;

namespace Account.Infrastructure.Consumers;

public sealed class CommitReservationCommandConsumer : IConsumer<SagaCommands.CommitReservationCommand>
{
    private readonly IMediator _mediator;
    private readonly ILogger<CommitReservationCommandConsumer> _logger;

    public CommitReservationCommandConsumer(
        IMediator mediator,
        ILogger<CommitReservationCommandConsumer> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Consume(ConsumeContext<SagaCommands.CommitReservationCommand> context)
    {
        _logger.LogInformation("Received CommitReservationCommand for Transfer {TransferId}, Reservation {ReservationId}", context.Message.TransferId, context.Message.ReservationId);

        var command = new CommitReservationCommand
        {
            TransferId = context.Message.TransferId,
            ReservationId = context.Message.ReservationId
        };

        var result = await _mediator.Send(command, context.CancellationToken);

        if (!result.Success)
        {
            _logger.LogWarning("Failed to commit reservation. TransferId: {TransferId}, Reason: {Reason}", context.Message.TransferId, result.FailureReason);

            return;
        }

        var transferCompletedEvent = new SagaEvents.TransferCompletedEvent
        {
            TransferId = context.Message.TransferId
        };

        await context.Publish(transferCompletedEvent, context.CancellationToken);

        _logger.LogInformation("Reservation committed successfully. Published TransferCompletedEvent for Transfer {TransferId}", context.Message.TransferId);
    }
}