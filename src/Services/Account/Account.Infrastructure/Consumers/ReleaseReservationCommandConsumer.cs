using Account.Application.Commands.ReleaseReservation;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using SagaCommands = Shared.Common.Sagas.Commands;
using SagaEvents = Shared.Common.Sagas.Events;

namespace Account.Infrastructure.Consumers;

public sealed class ReleaseReservationCommandConsumer : IConsumer<SagaCommands.ReleaseReservationCommand>
{
    private readonly IMediator _mediator;
    private readonly ILogger<ReleaseReservationCommandConsumer> _logger;

    public ReleaseReservationCommandConsumer(
        IMediator mediator,
        ILogger<ReleaseReservationCommandConsumer> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Consume(ConsumeContext<SagaCommands.ReleaseReservationCommand> context)
    {
        _logger.LogInformation("Received ReleaseReservationCommand for Transfer {TransferId}, Reservation {ReservationId}", context.Message.TransferId, context.Message.ReservationId);

        var command = new ReleaseReservationCommand
        {
            TransferId = context.Message.TransferId,
            ReservationId = context.Message.ReservationId
        };

        var result = await _mediator.Send(command, context.CancellationToken);

        if (!result.Success)
        {
            _logger.LogWarning("Failed to release reservation. TransferId: {TransferId}, Reason: {Reason}", context.Message.TransferId, result.FailureReason);

            return;
        }

        var transferCancelledEvent = new SagaEvents.TransferCancelledEvent
        {
            TransferId = context.Message.TransferId
        };

        await context.Publish(transferCancelledEvent, context.CancellationToken);

        _logger.LogInformation("Reservation released successfully. Published TransferCancelledEvent for Transfer {TransferId}", context.Message.TransferId);
    }
}