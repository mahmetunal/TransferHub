using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoneyTransfer.Domain.Entities;

namespace MoneyTransfer.Infrastructure.Persistence.Configurations;

public sealed class TransferConfiguration : IEntityTypeConfiguration<Transfer>
{
    public void Configure(EntityTypeBuilder<Transfer> builder)
    {
        builder.ToTable("Transfers");

        builder.HasKey(t => t.Id);

        builder.OwnsOne(t => t.SourceAccount, sourceAccount =>
        {
            sourceAccount.Property(sa => sa.Value)
                .HasMaxLength(34)
                .IsRequired();
        });

        builder.OwnsOne(t => t.DestinationAccount, destinationAccount =>
        {
            destinationAccount.Property(da => da.Value)
                .HasMaxLength(34)
                .IsRequired();
        });

        builder.OwnsOne(t => t.Amount, amount =>
        {
            amount.Property(a => a.Amount)
                .HasPrecision(18, 2)
                .IsRequired();

            amount.OwnsOne(a => a.Currency, currency =>
            {
                currency.Property(c => c.Code)
                    .HasMaxLength(3)
                    .IsRequired();
            });
        });

        builder.Property(t => t.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(t => t.FailureReason)
            .HasMaxLength(500);

        builder.Property(t => t.Reference)
            .HasMaxLength(255);

        builder.Property(t => t.RequestedAt)
            .IsRequired();

        builder.Property(t => t.CompletedAt);

        builder.HasIndex(t => t.Status);

        builder.HasIndex(t => t.RequestedAt);
    }
}