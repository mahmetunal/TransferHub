using Account.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Account.Infrastructure.Persistence.Configurations;

public sealed class BalanceReservationConfiguration : IEntityTypeConfiguration<BalanceReservation>
{
    public void Configure(EntityTypeBuilder<BalanceReservation> builder)
    {
        builder.ToTable("BalanceReservations");

        builder.HasKey(br => br.Id);

        builder.Property(br => br.AccountId)
            .IsRequired();

        builder.Property(br => br.TransferId)
            .IsRequired();

        builder.OwnsOne(br => br.Amount, amount =>
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

        builder.Property(br => br.ReservedAt)
            .IsRequired();

        builder.HasIndex(br => br.AccountId);

        builder.HasIndex(br => br.TransferId)
            .IsUnique();

        builder.HasIndex(br => new { br.AccountId, br.CommittedAt, br.ReleasedAt });
    }
}