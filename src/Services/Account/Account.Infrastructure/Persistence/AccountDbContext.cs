using Account.Domain.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Persistence;

namespace Account.Infrastructure.Persistence;

public class AccountDbContext : TransferHubDbContext
{
    public AccountDbContext(DbContextOptions<AccountDbContext> options) : base(options)
    {
    }

    public DbSet<Domain.Entities.Account> Accounts => Set<Domain.Entities.Account>();
    public DbSet<BalanceReservation> BalanceReservations => Set<BalanceReservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AccountDbContext).Assembly);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
    }
}