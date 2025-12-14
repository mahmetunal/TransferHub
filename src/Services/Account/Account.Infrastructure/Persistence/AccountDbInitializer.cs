using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Common.Persistence;
using Shared.Common.ValueObjects;
using Shared.Infrastructure.Persistence;

namespace Account.Infrastructure.Persistence;

public class AccountDbInitializer(
    AccountDbContext context,
    ILogger<AccountDbInitializer> logger) : IDbInitializer
{
    public async Task MigrateAsync(CancellationToken cancellationToken)
    {
        if ((await context.Database.GetPendingMigrationsAsync(cancellationToken)).Any())
        {
            logger.LogInformation("Applying pending migrations for AccountDb...");
            await context.Database.MigrateAsync(cancellationToken);
            logger.LogInformation("Migrations applied successfully for AccountDb");
        }
        else
        {
            logger.LogInformation("No pending migrations for AccountDb");
        }
    }

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (await context.Accounts.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Database already seeded - {Count} accounts found",
                await context.Accounts.CountAsync(cancellationToken));
            return;
        }

        logger.LogInformation("Starting Account database seeding...");

        // Mock users from AuthController:
        // - alice (user-alice-001) - alice@transferhub.com
        // - bob (user-bob-002) - bob@transferhub.com
        // - charlie (user-charlie-003) - charlie@transferhub.com

        // Seed Test Accounts with different currencies (using valid Turkish IBANs)
        var accounts = new List<Domain.Entities.Account>
        {
            Domain.Entities.Account.Create(
                Guid.NewGuid(),
                IBAN.Create("TR879236815113707898218269"),
                Money.Create(10000.00m, Currency.Create("TRY")),
                "user-alice-001"),

            Domain.Entities.Account.Create(
                Guid.NewGuid(),
                IBAN.Create("TR406907518428302681813695"),
                Money.Create(1000.00m, Currency.Create("USD")),
                "user-alice-001"),

            Domain.Entities.Account.Create(
                Guid.NewGuid(),
                IBAN.Create("TR270883384185653426183094"),
                Money.Create(5000.00m, Currency.Create("EUR")),
                "user-alice-001"),

            Domain.Entities.Account.Create(
                Guid.NewGuid(),
                IBAN.Create("TR364017786945493992709425"),
                Money.Create(5000.00m, Currency.Create("TRY")),
                "user-bob-002"),

            Domain.Entities.Account.Create(
                Guid.NewGuid(),
                IBAN.Create("TR505768089318490603374648"),
                Money.Create(2500.00m, Currency.Create("USD")),
                "user-bob-002"),

            Domain.Entities.Account.Create(
                Guid.NewGuid(),
                IBAN.Create("TR696907862214736633603940"),
                Money.Create(1500.00m, Currency.Create("GBP")),
                "user-bob-002"),

            Domain.Entities.Account.Create(
                Guid.NewGuid(),
                IBAN.Create("TR799197746829811615366924"),
                Money.Create(15000.00m, Currency.Create("TRY")),
                "user-charlie-003"),

            Domain.Entities.Account.Create(
                Guid.NewGuid(),
                IBAN.Create("TR767525983163920881672451"),
                Money.Create(3000.00m, Currency.Create("EUR")),
                "user-charlie-003")
        };

        await context.Accounts.AddRangeAsync(accounts, cancellationToken);

        await context.SaveChangesWithoutEventsAsync(cancellationToken: cancellationToken);

        logger.LogInformation("Database seeded successfully with {Count} accounts for {Users} users across {Currencies} currencies",
            accounts.Count,
            accounts.Select(a => a.OwnerId).Distinct().Count(),
            accounts.Select(a => a.Balance.Currency.Code).Distinct().Count());

        foreach (var ownerGroup in accounts.GroupBy(a => a.OwnerId).OrderBy(g => g.Key))
        {
            var ownerName = ownerGroup.Key switch
            {
                "user-alice-001" => "Alice",
                "user-bob-002" => "Bob",
                "user-charlie-003" => "Charlie",
                _ => ownerGroup.Key
            };

            logger.LogInformation("   👤 {Owner}: {Count} accounts",
                ownerName,
                ownerGroup.Count());

            foreach (var account in ownerGroup)
            {
                logger.LogInformation("      • {Currency} Account: {Balance:N2} ({IBAN})",
                    account.Balance.Currency.Code,
                    account.Balance.Amount,
                    account.Iban.Value);
            }
        }
    }
}