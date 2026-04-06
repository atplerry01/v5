using Whycespace.Runtime.EventFabric;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Runtime.Jobs.Chain;

/// <summary>
/// Dispatches chain-related alerts when anomalies, continuity breaks, or replay requirements are detected.
/// Triggers: anomaly detected, continuity break, replay required.
/// Alert routing is pluggable via IChainAlertSink.
/// </summary>
public sealed class ChainAlertDispatcher
{
    private readonly List<IChainAlertSink> _sinks = [];
    private readonly IEventPublisher? _eventPublisher;
    private readonly IClock _clock;

    public ChainAlertDispatcher(IClock clock, IEventPublisher? eventPublisher = null)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _eventPublisher = eventPublisher;
    }

    public ChainAlertDispatcher AddSink(IChainAlertSink sink)
    {
        ArgumentNullException.ThrowIfNull(sink);
        _sinks.Add(sink);
        return this;
    }

    public async Task DispatchAsync(
        ChainAlertType alertType,
        string description,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        var alert = new ChainAlert
        {
            AlertId = DeterministicIdHelper.FromSeed($"chain-alert:{alertType}:{correlationId}:{description}").ToString("N"),
            AlertType = alertType,
            Description = description,
            CorrelationId = correlationId,
            Timestamp = _clock.UtcNowOffset
        };

        // Dispatch to all registered sinks
        foreach (var sink in _sinks)
        {
            await sink.HandleAlertAsync(alert, cancellationToken);
        }

        // Emit alert as observability event
        if (_eventPublisher is not null)
        {
            await _eventPublisher.PublishAsync(new RuntimeEvent
            {
                EventId = DeterministicIdHelper.FromSeed($"chain-alert-event:{alertType}:{correlationId}:{description}"),
                AggregateId = Guid.Empty,
                AggregateType = "WhyceChain",
                EventType = alertType switch
                {
                    ChainAlertType.AnomalyDetected => "whyce.observability.chain.anomaly.detected",
                    ChainAlertType.ContinuityBreak => "whyce.observability.chain.anomaly.detected",
                    ChainAlertType.ReplayRequired => "whyce.observability.chain.recovery.triggered",
                    _ => "whyce.observability.chain.anomaly.detected"
                },
                CorrelationId = correlationId,
                Payload = alert,
                Timestamp = alert.Timestamp
            }, cancellationToken);
        }
    }
}

public enum ChainAlertType
{
    AnomalyDetected,
    ContinuityBreak,
    ReplayRequired
}

public sealed record ChainAlert
{
    public required string AlertId { get; init; }
    public required ChainAlertType AlertType { get; init; }
    public required string Description { get; init; }
    public required string CorrelationId { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
}

/// <summary>
/// Extension point for alert delivery (e.g., Slack, PagerDuty, email).
/// Implementations live in infrastructure/adapters.
/// </summary>
public interface IChainAlertSink
{
    Task HandleAlertAsync(ChainAlert alert, CancellationToken cancellationToken = default);
}
