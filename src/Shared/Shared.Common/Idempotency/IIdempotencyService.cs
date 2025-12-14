namespace Shared.Common.Idempotency;

public interface IIdempotencyService
{
    Task<(bool Exists, TResponse? Response)> TryGetAsync<TResponse>(string idempotencyKey, CancellationToken cancellationToken = default);

    Task SaveAsync<TResponse>(string idempotencyKey, TResponse response, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    Task RemoveAsync(string idempotencyKey, CancellationToken cancellationToken = default);
}