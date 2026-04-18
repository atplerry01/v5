namespace Whycespace.Shared.Contracts.Economic.Enforcement.Workflow;

/// <summary>
/// Intent DTO for triggering the enforcement lifecycle workflow.
/// Captures the violation-level data needed to drive the full chain:
/// violation → escalation → sanction → restriction → lock.
/// </summary>
public sealed record EnforcementLifecycleIntent(
    Guid ViolationId,
    Guid SubjectId,
    Guid RuleId,
    string Severity,
    string RecommendedAction,
    string Reason,
    Guid SanctionId,
    Guid RestrictionId,
    Guid LockId,
    DateTimeOffset DetectedAt);
