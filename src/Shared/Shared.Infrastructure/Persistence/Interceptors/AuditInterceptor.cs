using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Shared.Common.Domain;
using Shared.Common.Domain.Contracts;
using Shared.Common.Identity;

namespace Shared.Infrastructure.Persistence.Interceptors;

public class AuditInterceptor(
    ICurrentUser currentUser,
    TimeProvider timeProvider) : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateEntities(DbContext? context)
    {
        if (context == null) return;
        foreach (var entry in context.ChangeTracker.Entries<AuditableEntity>())
        {
            var utcNow = timeProvider.GetUtcNow();

            var hasOwnedEntities = entry.References.Any(r =>
                r.TargetEntry != null &&
                r.TargetEntry.Metadata.IsOwned() &&
                r.TargetEntry.State is EntityState.Added or EntityState.Modified);

            if (entry.State is EntityState.Added or EntityState.Modified || hasOwnedEntities)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedBy = currentUser.GetUserId();
                    entry.Entity.CreatedAt = utcNow;
                }

                entry.Entity.LastModifiedBy = currentUser.GetUserId();
                entry.Entity.LastModifiedAt = utcNow;
            }

            if (entry.State is not EntityState.Deleted || entry.Entity is not ISoftDeletable softDelete)
                continue;

            softDelete.DeletedBy = currentUser.GetUserId();
            softDelete.DeletedAt = utcNow;
            entry.State = EntityState.Modified;
        }
    }
}