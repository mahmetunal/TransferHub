using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Shared.Infrastructure.Extensions;

namespace Account.Infrastructure.Persistence;

public class AccountDbContextFactory : IDesignTimeDbContextFactory<AccountDbContext>
{
    public AccountDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AccountDbContext>();

        var config = new ConfigurationBuilder()
            .AddLocalJsonConfigurationFiles()
            .Build();

        optionsBuilder.UseNpgsql(config.GetConnectionString("PostgresDb"),
            npgsql => { npgsql.MigrationsAssembly(typeof(AccountDbContext).Assembly.GetName().Name); });

        return new AccountDbContext(optionsBuilder.Options);
    }
}