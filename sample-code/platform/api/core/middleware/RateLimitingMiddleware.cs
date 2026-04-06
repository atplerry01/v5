using System.Collections.Concurrent;
using Whycespace.Platform.Middleware;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Platform.Api.Core.Middleware;

/// <summary>
/// E19.17.7 — Rate Limiting and Abuse Protection Middleware.
///
/// Enforces:
/// - Per-identity rate limiting (requests per window)
/// - Per-endpoint throttling (endpoint-specific limits)
/// - Burst protection (short-window spike detection)
///
/// Uses a sliding window counter per identity + endpoint.
/// Non-blocking — rejected requests return 429 immediately.
/// </summary>
public sealed class RateLimitingMiddleware : IApiMiddleware
{
    private readonly RateLimitOptions _options;
    private readonly IClock _clock;
    private readonly IRateLimitStore _store;

    public RateLimitingMiddleware(RateLimitOptions options, IClock clock, IRateLimitStore store)
    {
        _options = options;
        _clock = clock;
        _store = store;
    }

    public async Task<ApiResponse> InvokeAsync(ApiRequest request, Func<ApiRequest, Task<ApiResponse>> next)
    {
        var identityId = request.WhyceId ?? "anonymous";
        var endpoint = request.Endpoint;

        // Check burst protection (short window)
        var burstKey = $"burst:{identityId}";
        var burstCount = await _store.IncrementAsync(burstKey, _options.BurstWindowSeconds);
        if (burstCount > _options.BurstLimit)
        {
            return TooManyRequests(request.TraceId, "Burst limit exceeded — slow down");
        }

        // Check per-identity rate limit (standard window)
        var identityKey = $"rate:{identityId}";
        var identityCount = await _store.IncrementAsync(identityKey, _options.WindowSeconds);
        if (identityCount > _options.MaxRequestsPerWindow)
        {
            return TooManyRequests(request.TraceId, "Rate limit exceeded — retry later");
        }

        // Check per-endpoint throttle
        var endpointKey = $"endpoint:{identityId}:{endpoint}";
        var endpointCount = await _store.IncrementAsync(endpointKey, _options.WindowSeconds);
        if (endpointCount > _options.MaxRequestsPerEndpoint)
        {
            return TooManyRequests(request.TraceId, "Endpoint throttle exceeded — retry later");
        }

        return await next(request);
    }

    private static ApiResponse TooManyRequests(string? traceId, string reason) =>
        new()
        {
            StatusCode = 429,
            Error = reason,
            TraceId = traceId
        };
}

/// <summary>
/// Rate limiting configuration.
/// </summary>
public sealed record RateLimitOptions
{
    /// <summary>Standard rate window in seconds (default: 60s).</summary>
    public int WindowSeconds { get; init; } = 60;

    /// <summary>Max requests per identity per window (default: 100).</summary>
    public int MaxRequestsPerWindow { get; init; } = 100;

    /// <summary>Max requests per identity per endpoint per window (default: 30).</summary>
    public int MaxRequestsPerEndpoint { get; init; } = 30;

    /// <summary>Burst window in seconds (default: 5s).</summary>
    public int BurstWindowSeconds { get; init; } = 5;

    /// <summary>Max requests in burst window (default: 15).</summary>
    public int BurstLimit { get; init; } = 15;

    public static RateLimitOptions Default => new();
}

/// <summary>
/// Rate limit counter store abstraction.
/// Implementations may use in-memory, Redis, or other distributed stores.
/// </summary>
public interface IRateLimitStore
{
    /// <summary>
    /// Increments the counter for a key within the given window.
    /// Returns the current count after increment.
    /// </summary>
    Task<int> IncrementAsync(string key, int windowSeconds);
}

/// <summary>
/// In-memory rate limit store for single-instance deployments.
/// Uses ConcurrentDictionary with TTL-based expiration.
/// </summary>
public sealed class InMemoryRateLimitStore : IRateLimitStore
{
    private readonly ConcurrentDictionary<string, RateLimitEntry> _entries = new();
    private readonly IClock _clock;

    public InMemoryRateLimitStore(IClock clock)
    {
        _clock = clock;
    }

    public Task<int> IncrementAsync(string key, int windowSeconds)
    {
        var now = _clock.UtcNowOffset;
        var windowStart = now.AddSeconds(-windowSeconds);

        var entry = _entries.AddOrUpdate(
            key,
            _ => new RateLimitEntry(1, now),
            (_, existing) =>
            {
                if (existing.WindowStart < windowStart)
                {
                    // Window expired — reset
                    return new RateLimitEntry(1, now);
                }

                return existing with { Count = existing.Count + 1 };
            });

        return Task.FromResult(entry.Count);
    }

    private sealed record RateLimitEntry(int Count, DateTimeOffset WindowStart);
}
