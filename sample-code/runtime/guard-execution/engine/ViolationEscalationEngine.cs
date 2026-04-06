using Microsoft.Extensions.Logging;
using Whycespace.Runtime.EventFabric;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Runtime.GuardExecution.Engine;

/// <summary>
/// Escalation engine for guard violations:
/// S0 → BLOCK + CRITICAL ALERT
/// S1 → BLOCK + ALERT
/// S2 → LOG + TRACK
/// S3 → METRIC
/// </summary>
public sealed class ViolationEscalationEngine
{
    private readonly IEventPublisher? _eventPublisher;
    private readonly IClock _clock;
    private readonly ILogger<ViolationEscalationEngine> _logger;

    public ViolationEscalationEngine(
        ILogger<ViolationEscalationEngine> logger,
        IClock clock,
        IEventPublisher? eventPublisher = null)
    {
        _logger = logger;
        _clock = clock;
        _eventPublisher = eventPublisher;
    }

    public async Task EscalateAsync(
        GuardExecutionReport report,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        foreach (var violation in report.AllViolations)
        {
            switch (violation.Severity)
            {
                case GuardSeverity.S0:
                    _logger.LogCritical(
                        "S0 CRITICAL VIOLATION [{Rule}] in {File}: {Description}",
                        violation.Rule, violation.File, violation.Description);
                    await PublishAlertAsync(violation, correlationId, "CRITICAL", cancellationToken);
                    break;

                case GuardSeverity.S1:
                    _logger.LogError(
                        "S1 VIOLATION [{Rule}] in {File}: {Description}",
                        violation.Rule, violation.File, violation.Description);
                    await PublishAlertAsync(violation, correlationId, "ALERT", cancellationToken);
                    break;

                case GuardSeverity.S2:
                    _logger.LogWarning(
                        "S2 WARNING [{Rule}] in {File}: {Description}",
                        violation.Rule, violation.File, violation.Description);
                    break;

                case GuardSeverity.S3:
                    _logger.LogInformation(
                        "S3 METRIC [{Rule}] in {File}: {Description}",
                        violation.Rule, violation.File, violation.Description);
                    break;
            }
        }
    }

    private async Task PublishAlertAsync(
        GuardViolation violation,
        string correlationId,
        string alertLevel,
        CancellationToken cancellationToken)
    {
        if (_eventPublisher is null) return;

        var alertEvent = new RuntimeEvent
        {
            EventId = DeterministicIdHelper.FromSeed($"guard:alert:{correlationId}:{violation.Rule}"),
            AggregateId = Guid.Empty,
            EventType = "GuardViolationAlertEvent",
            AggregateType = "GuardAudit",
            CorrelationId = correlationId,
            Payload = new
            {
                rule = violation.Rule,
                severity = violation.Severity.ToString(),
                file = violation.File,
                description = violation.Description,
                alertLevel,
                remediation = violation.Remediation
            },
            Timestamp = _clock.UtcNowOffset
        };

        await _eventPublisher.PublishAsync(alertEvent, cancellationToken);
    }
}
