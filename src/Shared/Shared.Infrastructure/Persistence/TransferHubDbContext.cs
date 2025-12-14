using Microsoft.EntityFrameworkCore;
using Shared.Common.Domain.Contracts;

namespace Shared.Infrastructure.Persistence;

public abstract class TransferHubDbContext : DbContext
{
    protected TransferHubDbContext()
    {
    }

    protected TransferHubDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.AppendGlobalQueryFilter<ISoftDeletable>(s => s.DeletedAt == null);
        base.OnModelCreating(modelBuilder);
    }
}