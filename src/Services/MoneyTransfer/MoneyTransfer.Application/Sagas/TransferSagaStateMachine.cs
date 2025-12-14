using MassTransit;
using Shared.Common.Sagas.Commands;
using Shared.Common.Sagas.Events;

namespace MoneyTransfer.Application.Sagas;

public sealed class TransferSagaStateMachine : MassTransitStateMachine<TransferSagaState>
{
    public TransferSagaStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => TransferInitiated, x => x.CorrelateById(context => context.Message.TransferId));
        Event(() => BalanceReserved, x => x.CorrelateById(context => context.Message.TransferId));
        Event(() => BalanceReservationFailed, x => x.CorrelateById(context => context.Message.TransferId));
        Event(() => DestinationCredited, x => x.CorrelateById(context => context.Message.TransferId));
        Event(() => CreditFailed, x => x.CorrelateById(context => context.Message.TransferId));
        Event(() => TransferCompleted, x => x.CorrelateById(context => context.Message.TransferId));
        Event(() => TransferCancelled, x => x.CorrelateById(context => context.Message.TransferId));

        Initially(
            When(TransferInitiated)
                .Then(context =>
                {
                    context.Saga.TransferId = context.Message.TransferId;
                    context.Saga.SourceAccount = context.Message.SourceAccount;
                    context.Saga.DestinationAccount = context.Message.DestinationAccount;
                    context.Saga.Amount = context.Message.Amount;
                    context.Saga.Currency = context.Message.Currency;
                    context.Saga.InitiatedBy = context.Message.InitiatedBy;
                    context.Saga.CreatedAt = DateTime.UtcNow;
                })
                .Publish(context => new ReserveBalanceCommand
                {
                    TransferId = context.Saga.TransferId,
                    AccountIban = context.Saga.SourceAccount,
                    Amount = context.Saga.Amount,
                    Currency = context.Saga.Currency,
                    InitiatedBy = context.Saga.InitiatedBy
                })
                .TransitionTo(ReservingBalance));

        During(ReservingBalance,
            When(BalanceReserved)
                .Then(context => { context.Saga.ReservationId = context.Message.ReservationId; })
                .Publish(context => new CreditAccountCommand
                {
                    TransferId = context.Saga.TransferId,
                    AccountIban = context.Saga.DestinationAccount,
                    Amount = context.Saga.Amount,
                    Currency = context.Saga.Currency
                })
                .TransitionTo(CreditingDestination),
            When(BalanceReservationFailed)
                .Then(context =>
                {
                    var reason = context.Message.Reason;

                    context.Saga.FailureReason = reason.Length > 500
                        ? reason[..497] + "..."
                        : reason;
                })
                .Publish(context => new CancelTransferCommand
                {
                    TransferId = context.Saga.TransferId,
                    Reason = context.Saga.FailureReason
                })
                .TransitionTo(Failed));

        During(CreditingDestination,
            When(DestinationCredited)
                .Publish(context => new CommitReservationCommand
                {
                    TransferId = context.Saga.TransferId,
                    ReservationId = context.Saga.ReservationId!.Value
                })
                .TransitionTo(CompletingTransfer),
            When(CreditFailed)
                .Then(context =>
                {
                    var reason = context.Message.Reason;

                    context.Saga.FailureReason = reason.Length > 500
                        ? reason[..497] + "..."
                        : reason;
                })
                .Publish(context => new ReleaseReservationCommand
                {
                    TransferId = context.Saga.TransferId,
                    ReservationId = context.Saga.ReservationId!.Value
                })
                .TransitionTo(RollingBack));

        During(CompletingTransfer,
            When(TransferCompleted)
                .Then(context => { context.Saga.CompletedAt = DateTime.UtcNow; })
                .Finalize());

        During(RollingBack,
            When(TransferCancelled)
                .Finalize());

        SetCompletedWhenFinalized();
    }

    public State ReservingBalance { get; private set; } = null!;
    public State CreditingDestination { get; private set; } = null!;
    public State CompletingTransfer { get; private set; } = null!;
    public State RollingBack { get; private set; } = null!;
    public State Failed { get; private set; } = null!;

    public Event<TransferInitiatedEvent> TransferInitiated { get; private set; } = null!;
    public Event<BalanceReservedEvent> BalanceReserved { get; private set; } = null!;
    public Event<BalanceReservationFailedEvent> BalanceReservationFailed { get; private set; } = null!;
    public Event<DestinationCreditedEvent> DestinationCredited { get; private set; } = null!;
    public Event<CreditFailedEvent> CreditFailed { get; private set; } = null!;
    public Event<TransferCompletedEvent> TransferCompleted { get; private set; } = null!;
    public Event<TransferCancelledEvent> TransferCancelled { get; private set; } = null!;
}