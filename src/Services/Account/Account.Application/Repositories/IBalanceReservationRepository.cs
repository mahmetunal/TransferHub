using Account.Domain.Entities;

namespace Account.Application.Repositories;

public interface IBalanceReservationRepository
{
    Task<BalanceReservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(BalanceReservation reservation, CancellationToken cancellationToken = default);
    Task UpdateAsync(BalanceReservation reservation, CancellationToken cancellationToken = default);
}