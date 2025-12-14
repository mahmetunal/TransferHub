using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Shared.Infrastructure.Extensions;

namespace MoneyTransfer.Infrastructure.Persistence;

public class MoneyTransferDbContextFactory : IDesignTimeDbContextFactory<MoneyTransferDbContext>
{
    public MoneyTransferDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MoneyTransferDbContext>();

        var config = new ConfigurationBuilder()
            .AddLocalJsonConfigurationFiles()
            .Build();

        optionsBuilder.UseNpgsql(config.GetConnectionString("PostgresDb"),
            npgsql => { npgsql.MigrationsAssembly(typeof(MoneyTransferDbContext).Assembly.GetName().Name); });

        return new MoneyTransferDbContext(optionsBuilder.Options);
    }
}