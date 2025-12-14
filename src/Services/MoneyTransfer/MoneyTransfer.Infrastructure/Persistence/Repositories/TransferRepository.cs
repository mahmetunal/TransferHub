using Microsoft.EntityFrameworkCore;
using MoneyTransfer.Application.Repositories;
using MoneyTransfer.Domain.Entities;

namespace MoneyTransfer.Infrastructure.Persistence.Repositories;

public sealed class TransferRepository : ITransferRepository
{
    private readonly MoneyTransferDbContext _context;

    public TransferRepository(MoneyTransferDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Transfer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Transfers
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<Transfer?> GetByIdWithEventsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Transfers
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Transfer>> GetByStatusAsync(TransferStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Transfers
            .AsNoTracking()
            .Where(t => t.Status == status)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Transfer>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Transfers
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Transfer transfer, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(transfer);

        await _context.Transfers.AddAsync(transfer, cancellationToken);
    }

    public Task UpdateAsync(Transfer transfer, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(transfer);

        _context.Transfers.Update(transfer);

        return Task.CompletedTask;
    }
}