using MoneyTransfer.Domain.Entities;

namespace MoneyTransfer.Application.Repositories;

/// <summary>
/// Repository interface for Transfer aggregate.
/// </summary>
public interface ITransferRepository
{
    Task<Transfer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Transfer?> GetByIdWithEventsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Transfer>> GetByStatusAsync(TransferStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Transfer>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Transfer transfer, CancellationToken cancellationToken = default);
    Task UpdateAsync(Transfer transfer, CancellationToken cancellationToken = default);
}