namespace Whycespace.Runtime.ControlPlane.CoreSystem;

/// <summary>
/// Handles governance escalation when core-system anomalies exceed thresholds.
/// Routes critical anomalies to the decision-system for governance response.
///
/// Escalation rules:
/// - Critical severity → immediate escalation (halt risk)
/// - High severity → alert escalation
/// - Medium severity → logged for periodic review
///
/// This is runtime orchestration — NOT domain logic.
/// </summary>
public sealed class GovernanceEscalationHandler
{
    private readonly IGovernanceEscalationTarget _target;

    public GovernanceEscalationHandler(IGovernanceEscalationTarget target)
    {
        ArgumentNullException.ThrowIfNull(target);
        _target = target;
    }

    public async Task HandleAnomalyAsync(CoreSystemAnomaly anomaly, CancellationToken cancellationToken = default)
    {
        var escalation = new GovernanceEscalation
        {
            AnomalyId = anomaly.AnomalyId,
            Severity = anomaly.Severity,
            CommandType = anomaly.CommandType,
            EntityId = anomaly.EntityId,
            Reason = anomaly.Reason,
            EscalationAction = DetermineAction(anomaly.Severity)
        };

        await _target.EscalateAsync(escalation, cancellationToken);
    }

    private static EscalationAction DetermineAction(AnomalySeverity severity) =>
        severity switch
        {
            AnomalySeverity.Critical => EscalationAction.Halt,
            AnomalySeverity.High => EscalationAction.Alert,
            AnomalySeverity.Medium => EscalationAction.Log,
            AnomalySeverity.Low => EscalationAction.Metric,
            _ => EscalationAction.Log
        };
}

public sealed record GovernanceEscalation
{
    public required Guid AnomalyId { get; init; }
    public required AnomalySeverity Severity { get; init; }
    public required string CommandType { get; init; }
    public required string EntityId { get; init; }
    public required string Reason { get; init; }
    public required EscalationAction EscalationAction { get; init; }
}

public enum EscalationAction
{
    Halt,
    Alert,
    Log,
    Metric
}

/// <summary>
/// Interface for routing governance escalations. Implemented by infrastructure.
/// </summary>
public interface IGovernanceEscalationTarget
{
    Task EscalateAsync(GovernanceEscalation escalation, CancellationToken cancellationToken = default);
}
