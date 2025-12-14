using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using Shared.Common.Domain.Contracts;
using Shared.Common.Events;

namespace Shared.Common.Domain;

public abstract class BaseEntity : IEntity
{
    public Guid Id { get; }

    protected BaseEntity()
    {
        Id = Guid.NewGuid();
    }

    protected BaseEntity(Guid id)
    {
        Id = id;
    }

    [NotMapped]
    public Collection<IDomainEvent> DomainEvents { get; } = [];

    public void RaiseDomainEvent(IDomainEvent @event)
    {
        if (!DomainEvents.Contains(@event))
            DomainEvents.Add(@event);
    }
}