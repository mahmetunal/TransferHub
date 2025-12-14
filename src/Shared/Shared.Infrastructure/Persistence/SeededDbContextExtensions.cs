using Microsoft.EntityFrameworkCore;
using Shared.Common.Domain.Contracts;

namespace Shared.Infrastructure.Persistence;

public static class SeededDbContextExtensions
{
    public static async Task SaveChangesWithoutEventsAsync(this DbContext context,
        CancellationToken cancellationToken = default)
    {
        var entities = context.ChangeTracker.Entries<IEntity>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Count > 0)
            .ToList();

        foreach (var entity in entities)
        {
            entity.DomainEvents.Clear();
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}