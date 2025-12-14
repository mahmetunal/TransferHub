using Account.Application.Repositories;
using Account.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Account.Infrastructure.Persistence.Repositories;

public sealed class BalanceReservationRepository : IBalanceReservationRepository
{
    private readonly AccountDbContext _context;

    public BalanceReservationRepository(AccountDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<BalanceReservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.BalanceReservations
            .AsNoTracking()
            .FirstOrDefaultAsync(br => br.Id == id, cancellationToken);
    }

    public async Task AddAsync(BalanceReservation reservation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reservation);

        await _context.BalanceReservations.AddAsync(reservation, cancellationToken);
    }

    public Task UpdateAsync(BalanceReservation reservation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reservation);

        _context.BalanceReservations.Update(reservation);
        return Task.CompletedTask;
    }
}