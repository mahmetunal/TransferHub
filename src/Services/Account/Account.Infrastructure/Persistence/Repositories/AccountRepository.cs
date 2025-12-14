using Account.Application.Repositories;
using Microsoft.EntityFrameworkCore;
using AccountEntity = Account.Domain.Entities.Account;

namespace Account.Infrastructure.Persistence.Repositories;

public sealed class AccountRepository : IAccountRepository
{
    private readonly AccountDbContext _context;

    public AccountRepository(AccountDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<AccountEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<AccountEntity?> GetByIbanAsync(string iban, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(iban))
            throw new ArgumentException("IBAN cannot be null or empty", nameof(iban));

        var normalizedIban = iban.Replace(" ", "").Replace("-", "").ToUpperInvariant();

        return await _context.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Iban.Value == normalizedIban, cancellationToken);
    }

    public async Task<AccountEntity?> GetByIbanWithReservationsAsync(string iban, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(iban))
            throw new ArgumentException("IBAN cannot be null or empty", nameof(iban));

        var normalizedIban = iban.Replace(" ", "").Replace("-", "").ToUpperInvariant();

        return await _context.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Iban.Value == normalizedIban, cancellationToken);
    }

    public async Task<List<AccountEntity>> GetByOwnerIdAsync(string ownerId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ownerId))
            throw new ArgumentException("Owner ID cannot be null or empty", nameof(ownerId));

        return await _context.Accounts
            .AsNoTracking()
            .Where(a => a.OwnerId == ownerId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(AccountEntity account, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(account);

        await _context.Accounts.AddAsync(account, cancellationToken);
    }

    public Task UpdateAsync(AccountEntity account, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(account);

        _context.Accounts.Update(account);
        return Task.CompletedTask;
    }
}