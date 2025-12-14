using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoneyTransfer.Application.Sagas;

namespace MoneyTransfer.Infrastructure.Persistence.Configurations;

public class TransferSagaStateConfiguration : IEntityTypeConfiguration<TransferSagaState>
{
    public void Configure(EntityTypeBuilder<TransferSagaState> builder)
    {
        builder.ToTable("TransferSagaState");

        builder.HasKey(x => x.CorrelationId);

        builder.HasIndex(x => x.TransferId)
            .IsUnique();

        builder.HasIndex(x => x.CurrentState);

        builder.HasIndex(x => x.CreatedAt);

        builder.Property(x => x.CorrelationId)
            .IsRequired();

        builder.Property(x => x.CurrentState)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.TransferId)
            .IsRequired();

        builder.Property(x => x.SourceAccount)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.DestinationAccount)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(x => x.ReservationId);

        builder.Property(x => x.FailureReason)
            .HasMaxLength(500);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.CompletedAt);

        builder.Property(x => x.InitiatedBy)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.RowVersion)
            .IsRowVersion();
    }
}