using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Account.Infrastructure.Persistence.Configurations;

public sealed class AccountConfiguration : IEntityTypeConfiguration<Domain.Entities.Account>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Account> builder)
    {
        builder.ToTable("Accounts");

        builder.HasKey(a => a.Id);

        builder.OwnsOne(a => a.Iban, iban =>
        {
            iban.Property(i => i.Value)
                .HasMaxLength(34)
                .IsRequired();

            iban.HasIndex(i => i.Value)
                .IsUnique();
        });

        builder.OwnsOne(a => a.Balance, balance =>
        {
            balance.Property(b => b.Amount)
                .HasPrecision(18, 2)
                .IsRequired();

            balance.OwnsOne(b => b.Currency, currency =>
            {
                currency.Property(c => c.Code)
                    .HasMaxLength(3)
                    .IsRequired();
            });
        });

        builder.Property(a => a.OwnerId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.IsActive)
            .IsRequired();

        builder.HasIndex(a => a.OwnerId);

        builder.HasIndex(a => a.IsActive);
    }
}