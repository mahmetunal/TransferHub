using AccountEntity = Account.Domain.Entities.Account;

namespace Account.Application.Repositories;

public interface IAccountRepository
{
    Task<AccountEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AccountEntity?> GetByIbanAsync(string iban, CancellationToken cancellationToken = default);
    Task<AccountEntity?> GetByIbanWithReservationsAsync(string iban, CancellationToken cancellationToken = default);
    Task<List<AccountEntity>> GetByOwnerIdAsync(string ownerId, CancellationToken cancellationToken = default);
    Task AddAsync(AccountEntity account, CancellationToken cancellationToken = default);
    Task UpdateAsync(AccountEntity account, CancellationToken cancellationToken = default);
}