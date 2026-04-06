namespace Whycespace.Domain.ConstitutionalSystem.Policy.Enforcement;

public sealed class EnforcementAction
{
    public Guid ActionId { get; }
    public EnforcementType Type { get; }
    public EnforcementSeverity Severity { get; }
    public EnforcementTargetType TargetType { get; }
    public string TargetId { get; }
    public string Reason { get; }
    public string CommandCorrelationId { get; }
    public DateTimeOffset CreatedAt { get; }

    private EnforcementAction(
        Guid actionId,
        EnforcementType type,
        EnforcementSeverity severity,
        EnforcementTargetType targetType,
        string targetId,
        string reason,
        string commandCorrelationId,
        DateTimeOffset createdAt)
    {
        ActionId = actionId;
        Type = type;
        Severity = severity;
        TargetType = targetType;
        TargetId = targetId;
        Reason = reason;
        CommandCorrelationId = commandCorrelationId;
        CreatedAt = createdAt;
    }

    public static EnforcementAction Create(
        Guid actionId,
        EnforcementType type,
        EnforcementSeverity severity,
        EnforcementTargetType targetType,
        string targetId,
        string reason,
        string correlationId,
        DateTimeOffset timestamp)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(severity);
        ArgumentNullException.ThrowIfNull(targetType);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetId);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        return new EnforcementAction(
            actionId, type, severity, targetType, targetId,
            reason, correlationId, timestamp);
    }

    public string ToCommandType() => $"enforcement.{TargetType.Value}.{Type.Value}";
}
