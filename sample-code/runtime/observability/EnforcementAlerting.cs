using Microsoft.Extensions.Logging;
using Whycespace.Runtime.EventFabric;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Runtime.Observability;

/// <summary>
/// Enforcement-level alerting integration.
/// Emits alerts as RuntimeEvents for S0/S1 enforcement violations.
///
/// Delegates to IEventPublisher (Kafka via outbox) for delivery.
/// External integrations (PagerDuty, Slack) are handled by event consumers.
///
/// All methods are non-blocking and non-throwing.
/// </summary>
public sealed class EnforcementAlerting
{
    private readonly IEventPublisher? _eventPublisher;
    private readonly MetricsCollector _metrics;
    private readonly IClock _clock;
    private readonly ILogger<EnforcementAlerting> _logger;

    public EnforcementAlerting(
        MetricsCollector metrics,
        IClock clock,
        ILogger<EnforcementAlerting> logger,
        IEventPublisher? eventPublisher = null)
    {
        _metrics = metrics;
        _clock = clock;
        _logger = logger;
        _eventPublisher = eventPublisher;
    }

    /// <summary>
    /// Raise an enforcement alert. Non-blocking, non-throwing.
    /// Only S0 (CRITICAL) and S1 (ALERT) should trigger alerts.
    /// </summary>
    public void Raise(string level, string message, string correlationId, string? rule = null)
    {
        try
        {
            _metrics.Increment($"runtime.enforcement.alert.{level.ToLowerInvariant()}",
                new Dictionary<string, string>
                {
                    ["level"] = level,
                    ["rule"] = rule ?? "unknown"
                });

            _logger.LogWarning(
                "Enforcement alert [{Level}]: {Message} correlation={CorrelationId} rule={Rule}",
                level, message, correlationId, rule ?? "none");

            if (_eventPublisher is not null)
            {
                _ = PublishAlertAsync(level, message, correlationId, rule);
            }
        }
        catch
        {
            // Non-throwing — alerting must never block execution
        }
    }

    private async Task PublishAlertAsync(string level, string message, string correlationId, string? rule)
    {
        try
        {
            await _eventPublisher!.PublishAsync(new RuntimeEvent
            {
                EventId = DeterministicIdHelper.FromSeed(
                    $"enforcement-alert:{level}:{correlationId}:{rule ?? "none"}"),
                AggregateId = Guid.Empty,
                AggregateType = "EnforcementAudit",
                EventType = "whyce.observability.enforcement.alert.raised",
                CorrelationId = correlationId,
                Payload = new
                {
                    level,
                    message,
                    rule
                },
                Timestamp = _clock.UtcNowOffset
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish enforcement alert event");
        }
    }

    public static class AlertLevels
    {
        public const string Critical = "CRITICAL";
        public const string Alert = "ALERT";
    }
}
