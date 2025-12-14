using Shared.Common.Events;

namespace Account.Domain.Events;

public sealed class AccountDeactivatedEvent : IDomainEvent
{
    public Guid Id { get; }
    public DateTime OccurredOn { get; }
    public Guid AccountId { get; }

    public AccountDeactivatedEvent(Guid accountId, DateTime occurredOn)
    {
        Id = Guid.NewGuid();
        AccountId = accountId;
        OccurredOn = occurredOn;
    }
}