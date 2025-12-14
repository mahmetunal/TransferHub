using MassTransit;

namespace MoneyTransfer.Application.Sagas;

public class TransferSagaState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = null!;
    public Guid TransferId { get; set; }
    public string SourceAccount { get; set; } = null!;
    public string DestinationAccount { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = null!;
    public string InitiatedBy { get; set; } = null!;
    public Guid? ReservationId { get; set; }
    public string? FailureReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public byte[]? RowVersion { get; set; }
}