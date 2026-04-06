using Microsoft.Extensions.Logging;
using Whycespace.Runtime.EventFabric;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Runtime.Observability;

/// <summary>
/// Emits enforcement anomaly signals as observability events.
/// Routes to T3I pipeline (WhyceAtlas / GovernanceAssist).
///
/// All emission is non-blocking and non-throwing — safe for hot-path use.
/// </summary>
public sealed class EnforcementAnomalyEmitter
{
    private readonly IEventPublisher? _eventPublisher;
    private readonly MetricsCollector _metrics;
    private readonly IClock _clock;
    private readonly ILogger<EnforcementAnomalyEmitter> _logger;

    public EnforcementAnomalyEmitter(
        MetricsCollector metrics,
        IClock clock,
        ILogger<EnforcementAnomalyEmitter> logger,
        IEventPublisher? eventPublisher = null)
    {
        _metrics = metrics;
        _clock = clock;
        _logger = logger;
        _eventPublisher = eventPublisher;
    }

    /// <summary>
    /// Emit an anomaly signal. Non-blocking, non-throwing.
    /// </summary>
    public void Emit(EnforcementAnomalySignal signal)
    {
        try
        {
            _metrics.Increment(EnforcementMetrics.Names.AnomalyEmitted, new Dictionary<string, string>
            {
                ["type"] = signal.Type,
                ["command_type"] = signal.CommandType ?? "unknown"
            });

            _logger.LogWarning(
                "Enforcement anomaly: type={Type} correlation={CorrelationId} — {Description}",
                signal.Type, signal.CorrelationId, signal.Description);

            // Fire-and-forget event emission (non-blocking)
            if (_eventPublisher is not null)
            {
                _ = PublishAnomalyEventAsync(signal);
            }
        }
        catch
        {
            // Non-throwing — anomaly emission must never block execution
        }
    }

    private async Task PublishAnomalyEventAsync(EnforcementAnomalySignal signal)
    {
        try
        {
            await _eventPublisher!.PublishAsync(new RuntimeEvent
            {
                EventId = DeterministicIdHelper.FromSeed(
                    $"enforcement-anomaly:{signal.Type}:{signal.CorrelationId}"),
                AggregateId = Guid.Empty,
                AggregateType = "GovernanceAssist",
                EventType = "whyce.observability.enforcement.anomaly.detected",
                CorrelationId = signal.CorrelationId,
                Payload = new
                {
                    type = signal.Type,
                    description = signal.Description,
                    commandType = signal.CommandType,
                    shardId = signal.ShardId
                },
                Timestamp = signal.Timestamp
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish anomaly event for {Type}", signal.Type);
        }
    }

    // Well-known anomaly types
    public static class AnomalyTypes
    {
        public const string GuardFailureSpike = "GUARD_FAILURE_SPIKE";
        public const string PolicyDrift = "POLICY_DRIFT";
        public const string ChainAnchorFailure = "CHAIN_ANCHOR_FAILURE";
        public const string ReplayDivergence = "REPLAY_DIVERGENCE";
    }
}
