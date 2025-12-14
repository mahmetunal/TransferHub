using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Shared.Common.Idempotency;

namespace Shared.Infrastructure.Idempotency;

public sealed class RedisIdempotencyService : IIdempotencyService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<RedisIdempotencyService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    private const string KeyPrefix = "idempotency:";
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromHours(24);

    public RedisIdempotencyService(
        IDistributedCache cache,
        ILogger<RedisIdempotencyService> logger)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };
    }

    public async Task<(bool Exists, TResponse? Response)> TryGetAsync<TResponse>(
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        ValidateIdempotencyKey(idempotencyKey);

        try
        {
            var cacheKey = GetCacheKey(idempotencyKey);
            var cachedData = await _cache.GetStringAsync(cacheKey, cancellationToken);

            if (string.IsNullOrEmpty(cachedData))
            {
                _logger.LogDebug(
                    "Idempotency key {IdempotencyKey} not found in cache",
                    idempotencyKey);
                return (false, default);
            }

            var response = JsonSerializer.Deserialize<TResponse>(cachedData, _jsonOptions);

            _logger.LogInformation(
                "Retrieved cached response for idempotency key {IdempotencyKey}",
                idempotencyKey);

            return (true, response);
        }
        catch (JsonException ex)
        {
            _logger.LogError(
                ex,
                "Failed to deserialize cached response for idempotency key {IdempotencyKey}",
                idempotencyKey);

            await RemoveAsync(idempotencyKey, cancellationToken);

            return (false, default);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving idempotent response for key {IdempotencyKey}",
                idempotencyKey);
            throw;
        }
    }

    public async Task SaveAsync<TResponse>(
        string idempotencyKey,
        TResponse response,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        ValidateIdempotencyKey(idempotencyKey);

        if (response == null)
        {
            throw new ArgumentNullException(nameof(response), "Response cannot be null");
        }

        try
        {
            var cacheKey = GetCacheKey(idempotencyKey);
            var serializedResponse = JsonSerializer.Serialize(response, _jsonOptions);

            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? DefaultExpiration
            };

            await _cache.SetStringAsync(cacheKey, serializedResponse, cacheOptions, cancellationToken);

            _logger.LogInformation(
                "Saved idempotent response for key {IdempotencyKey} with expiration {Expiration}",
                idempotencyKey,
                expiration ?? DefaultExpiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error saving idempotent response for key {IdempotencyKey}",
                idempotencyKey);
            throw;
        }
    }

    public async Task RemoveAsync(string idempotencyKey, CancellationToken cancellationToken = default)
    {
        ValidateIdempotencyKey(idempotencyKey);

        try
        {
            var cacheKey = GetCacheKey(idempotencyKey);
            await _cache.RemoveAsync(cacheKey, cancellationToken);

            _logger.LogInformation(
                "Removed cached response for idempotency key {IdempotencyKey}",
                idempotencyKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error removing idempotent response for key {IdempotencyKey}",
                idempotencyKey);
            throw;
        }
    }


    private static string GetCacheKey(string idempotencyKey)
    {
        return $"{KeyPrefix}{idempotencyKey}";
    }

    private static void ValidateIdempotencyKey(string idempotencyKey)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new ArgumentException("Idempotency key cannot be null or whitespace", nameof(idempotencyKey));
        }

        if (idempotencyKey.Length > 255)
        {
            throw new ArgumentException("Idempotency key cannot exceed 255 characters", nameof(idempotencyKey));
        }
    }
}