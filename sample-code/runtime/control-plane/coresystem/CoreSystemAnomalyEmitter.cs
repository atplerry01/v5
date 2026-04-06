using Whycespace.Runtime.ControlPlane.Middleware;

namespace Whycespace.Runtime.ControlPlane.CoreSystem;

/// <summary>
/// Runtime-layer implementation of core-system anomaly emission.
/// Emits structured anomaly events when post-execution verification
/// detects invariant breaches.
///
/// Hardened pipeline:
/// 1. Deduplication — suppress duplicate anomalies within window
/// 2. Rate limiting — prevent alert flooding
/// 3. Aggregation — batch similar anomalies
/// 4. Emit — record to anomaly store
///
/// Critical severity anomalies ALWAYS bypass dedup and rate limits
/// to ensure governance escalation is never suppressed.
///
/// This is runtime infrastructure — NOT domain logic.
/// </summary>
public sealed class CoreSystemAnomalyEmitter : ICoreSystemAnomalyEmitter
{
    private readonly IAnomalyEventStore _eventStore;
    private readonly AnomalyDeduplicator _deduplicator;
    private readonly AnomalyRateLimiter _rateLimiter;
    private readonly AnomalyAggregator _aggregator;

    public CoreSystemAnomalyEmitter(
        IAnomalyEventStore eventStore,
        AnomalyDeduplicator deduplicator,
        AnomalyRateLimiter rateLimiter,
        AnomalyAggregator aggregator)
    {
        ArgumentNullException.ThrowIfNull(eventStore);
        ArgumentNullException.ThrowIfNull(deduplicator);
        ArgumentNullException.ThrowIfNull(rateLimiter);
        ArgumentNullException.ThrowIfNull(aggregator);
        _eventStore = eventStore;
        _deduplicator = deduplicator;
        _rateLimiter = rateLimiter;
        _aggregator = aggregator;
    }

    public async Task EmitAnomalyAsync(
        Guid commandId,
        string commandType,
        string entityId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var anomaly = new CoreSystemAnomaly
        {
            AnomalyId = commandId,
            CommandType = commandType,
            EntityId = entityId,
            Reason = reason,
            Severity = ClassifySeverity(reason),
            DetectedAt = DateTimeOffset.MinValue // Set by runtime clock
        };

        // Critical anomalies always emit — never suppressed
        if (anomaly.Severity == AnomalySeverity.Critical)
        {
            await _eventStore.RecordAsync(anomaly, cancellationToken);
            return;
        }

        // Deduplication: suppress if identical anomaly was recently emitted
        if (!_deduplicator.ShouldEmit(anomaly))
            return;

        // Rate limiting: suppress if rate limit exceeded
        if (!_rateLimiter.TryAcquire())
        {
            // Still aggregate for periodic summary
            _aggregator.Record(anomaly);
            return;
        }

        await _eventStore.RecordAsync(anomaly, cancellationToken);
    }

    private static AnomalySeverity ClassifySeverity(string reason)
    {
        if (reason.Contains("negative", StringComparison.OrdinalIgnoreCase)
            || reason.Contains("imbalance", StringComparison.OrdinalIgnoreCase))
            return AnomalySeverity.Critical;

        if (reason.Contains("inconsistent", StringComparison.OrdinalIgnoreCase)
            || reason.Contains("drift", StringComparison.OrdinalIgnoreCase))
            return AnomalySeverity.High;

        return AnomalySeverity.Medium;
    }
}

public sealed record CoreSystemAnomaly
{
    public required Guid AnomalyId { get; init; }
    public required string CommandType { get; init; }
    public required string EntityId { get; init; }
    public required string Reason { get; init; }
    public required AnomalySeverity Severity { get; init; }
    public required DateTimeOffset DetectedAt { get; init; }
}

public enum AnomalySeverity
{
    Critical,
    High,
    Medium,
    Low
}

/// <summary>
/// Interface for recording anomaly events. Implemented by infrastructure.
/// </summary>
public interface IAnomalyEventStore
{
    Task RecordAsync(CoreSystemAnomaly anomaly, CancellationToken cancellationToken = default);
}
