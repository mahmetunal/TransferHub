using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MoneyTransfer.Domain.Entities;
using Shared.Common.Persistence;
using Shared.Common.ValueObjects;
using Shared.Infrastructure.Persistence;

namespace MoneyTransfer.Infrastructure.Persistence;

public class MoneyTransferDbInitializer(
    MoneyTransferDbContext context,
    ILogger<MoneyTransferDbInitializer> logger) : IDbInitializer
{
    public async Task MigrateAsync(CancellationToken cancellationToken)
    {
        if ((await context.Database.GetPendingMigrationsAsync(cancellationToken)).Any())
        {
            logger.LogInformation("Applying pending migrations for MoneyTransferDb...");
            await context.Database.MigrateAsync(cancellationToken);
            logger.LogInformation("Migrations applied successfully for MoneyTransferDb");
        }
        else
        {
            logger.LogInformation("No pending migrations for MoneyTransferDb");
        }
    }

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (await context.Transfers.AnyAsync(cancellationToken))
        {
            var count = await context.Transfers.CountAsync(cancellationToken);

            logger.LogInformation("Database already seeded - {Count} transfers found", count);

            return;
        }

        logger.LogInformation("Starting MoneyTransfer database seeding...");

        // Mock users from AuthController:
        // - alice (user-alice-001): TR879236815113707898218269 (TRY), TR406907518428302681813695 (USD), TR270883384185653426183094 (EUR)
        // - bob (user-bob-002): TR364017786945493992709425 (TRY), TR505768089318490603374648 (USD), TR696907862214736633603940 (GBP)
        // - charlie (user-charlie-003): TR799197746829811615366924 (TRY), TR767525983163920881672451 (EUR)

        var transfers = new List<Transfer>
        {
            Transfer.Create(
                Guid.NewGuid(),
                IBAN.Create("TR879236815113707898218269"),
                IBAN.Create("TR364017786945493992709425"),
                Money.Create(500.00m, Currency.Create("TRY")),
                "user-alice-001",
                "Payment for services"
            ),

            Transfer.Create(
                Guid.NewGuid(),
                IBAN.Create("TR505768089318490603374648"),
                IBAN.Create("TR406907518428302681813695"),
                Money.Create(100.00m, Currency.Create("USD")),
                "user-bob-002",
                "USD payment"
            ),

            Transfer.Create(
                Guid.NewGuid(),
                IBAN.Create("TR767525983163920881672451"),
                IBAN.Create("TR270883384185653426183094"),
                Money.Create(250.00m, Currency.Create("EUR")),
                "user-charlie-003",
                "EUR transfer"),

            Transfer.Create(
                Guid.NewGuid(),
                IBAN.Create("TR879236815113707898218269"),
                IBAN.Create("TR799197746829811615366924"),
                Money.Create(1000.00m, Currency.Create("TRY")),
                "user-alice-001",
                "Payment for contract"
            ),

            Transfer.Create(
                Guid.NewGuid(),
                IBAN.Create("TR505768089318490603374648"),
                IBAN.Create("TR406907518428302681813695"),
                Money.Create(50.00m, Currency.Create("USD")),
                "user-bob-002",
                "Refund")
        };

        await context.Transfers.AddRangeAsync(transfers, cancellationToken);

        await context.SaveChangesWithoutEventsAsync(cancellationToken: cancellationToken);

        logger.LogInformation("Database seeded successfully with {Count} transfers", transfers.Count);

        logger.LogInformation("Initiated by Alice: {AliceCount}", transfers.Count(t => t.InitiatedBy == "user-alice-001"));
        logger.LogInformation("Initiated by Bob: {BobCount}", transfers.Count(t => t.InitiatedBy == "user-bob-002"));
        logger.LogInformation("Initiated by Charlie: {CharlieCount}", transfers.Count(t => t.InitiatedBy == "user-charlie-003"));

        logger.LogInformation("Total Amount: TRY {TRY:N2}, USD {USD:N2}, EUR {EUR:N2}",
            transfers.Where(t => t.Amount.Currency.Code == "TRY").Sum(t => t.Amount.Amount),
            transfers.Where(t => t.Amount.Currency.Code == "USD").Sum(t => t.Amount.Amount),
            transfers.Where(t => t.Amount.Currency.Code == "EUR").Sum(t => t.Amount.Amount));
    }
}