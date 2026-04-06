using System.Collections.Concurrent;
using Whycespace.Runtime.Command;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Runtime.ControlPlane.Middleware;

/// <summary>
/// Per-identity and per-endpoint rate limiting middleware.
/// Uses a sliding window algorithm with deterministic key derivation.
/// Placed AFTER context resolution, BEFORE policy enforcement in the pipeline.
/// </summary>
public sealed class RateLimitingMiddleware : IMiddleware
{
    private readonly IRateLimitConfiguration _config;
    private readonly IClock _clock;
    private readonly ConcurrentDictionary<string, RateLimitBucket> _buckets = new();

    public RateLimitingMiddleware(IRateLimitConfiguration config, IClock clock)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public Task<CommandResult> InvokeAsync(CommandContext context, MiddlewareDelegate next)
    {
        var identityKey = ResolveIdentityKey(context);
        var endpointKey = context.Envelope.CommandType;

        // Check per-identity limit
        if (!string.IsNullOrEmpty(identityKey))
        {
            var identityBucketKey = $"identity:{identityKey}";
            if (IsRateLimited(identityBucketKey, _config.MaxRequestsPerIdentity, _config.IdentityWindowDuration))
            {
                return Task.FromResult(CommandResult.Fail(
                    context.Envelope.CommandId,
                    "RATE_LIMIT_EXCEEDED",
                    $"Rate limit exceeded for identity '{identityKey}'. Max {_config.MaxRequestsPerIdentity} requests per {_config.IdentityWindowDuration.TotalSeconds}s."));
            }
        }

        // Check per-endpoint limit
        var endpointBucketKey = $"endpoint:{endpointKey}";
        if (IsRateLimited(endpointBucketKey, _config.MaxRequestsPerEndpoint, _config.EndpointWindowDuration))
        {
            return Task.FromResult(CommandResult.Fail(
                context.Envelope.CommandId,
                "RATE_LIMIT_EXCEEDED",
                $"Rate limit exceeded for endpoint '{endpointKey}'. Max {_config.MaxRequestsPerEndpoint} requests per {_config.EndpointWindowDuration.TotalSeconds}s."));
        }

        // Check burst protection (global)
        var globalKey = "global:burst";
        if (IsRateLimited(globalKey, _config.MaxBurstRequests, _config.BurstWindowDuration))
        {
            return Task.FromResult(CommandResult.Fail(
                context.Envelope.CommandId,
                "BURST_LIMIT_EXCEEDED",
                "Global burst limit exceeded. Please retry later."));
        }

        return next(context);
    }

    private bool IsRateLimited(string bucketKey, int maxRequests, TimeSpan window)
    {
        var now = _clock.UtcNowOffset;
        var bucket = _buckets.GetOrAdd(bucketKey, _ => new RateLimitBucket());

        lock (bucket)
        {
            // Evict entries outside the sliding window
            var cutoff = now - window;
            while (bucket.Timestamps.Count > 0 && bucket.Timestamps.Peek() < cutoff)
            {
                bucket.Timestamps.Dequeue();
            }

            if (bucket.Timestamps.Count >= maxRequests)
                return true;

            bucket.Timestamps.Enqueue(now);
            return false;
        }
    }

    private static string ResolveIdentityKey(CommandContext context)
    {
        // WhyceId from command metadata is the canonical identity key
        return context.Envelope.Metadata.WhyceId ?? string.Empty;
    }

    private sealed class RateLimitBucket
    {
        public Queue<DateTimeOffset> Timestamps { get; } = new();
    }
}

/// <summary>
/// Rate limit configuration contract. Injected by the runtime host.
/// </summary>
public interface IRateLimitConfiguration
{
    int MaxRequestsPerIdentity { get; }
    TimeSpan IdentityWindowDuration { get; }
    int MaxRequestsPerEndpoint { get; }
    TimeSpan EndpointWindowDuration { get; }
    int MaxBurstRequests { get; }
    TimeSpan BurstWindowDuration { get; }
}

/// <summary>
/// Default rate limit configuration — sensible production defaults.
/// </summary>
public sealed class DefaultRateLimitConfiguration : IRateLimitConfiguration
{
    public int MaxRequestsPerIdentity { get; init; } = 100;
    public TimeSpan IdentityWindowDuration { get; init; } = TimeSpan.FromMinutes(1);
    public int MaxRequestsPerEndpoint { get; init; } = 500;
    public TimeSpan EndpointWindowDuration { get; init; } = TimeSpan.FromMinutes(1);
    public int MaxBurstRequests { get; init; } = 1000;
    public TimeSpan BurstWindowDuration { get; init; } = TimeSpan.FromSeconds(10);
}
