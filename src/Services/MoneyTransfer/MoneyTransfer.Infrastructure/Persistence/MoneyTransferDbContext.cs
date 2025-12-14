using MassTransit;
using Microsoft.EntityFrameworkCore;
using MoneyTransfer.Domain.Entities;
using Shared.Infrastructure.Persistence;

namespace MoneyTransfer.Infrastructure.Persistence;

public class MoneyTransferDbContext : TransferHubDbContext
{
    public MoneyTransferDbContext(DbContextOptions<MoneyTransferDbContext> options) : base(options)
    {
    }

    public DbSet<Transfer> Transfers => Set<Transfer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MoneyTransferDbContext).Assembly);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
    }
}