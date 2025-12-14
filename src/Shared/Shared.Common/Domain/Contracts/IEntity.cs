using System.Collections.ObjectModel;
using Shared.Common.Events;

namespace Shared.Common.Domain.Contracts;

public interface IEntity
{
    public Guid Id { get; }
    Collection<IDomainEvent> DomainEvents { get; }
}