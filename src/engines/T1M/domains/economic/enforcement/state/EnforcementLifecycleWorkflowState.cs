namespace Whycespace.Engines.T1M.Domains.Economic.Enforcement.State;

/// <summary>
/// State carried through the enforcement lifecycle workflow.
/// Tracks IDs for each stage: violation → escalation → sanction → restriction → lock.
/// Idempotent: all IDs are pre-assigned at workflow start.
///
/// The workflow progresses through escalation tiers:
///   - Low/Medium severity: escalate + sanction only (no restriction or lock).
///   - High severity: escalate + sanction + restriction.
///   - Critical severity: escalate + sanction + restriction + lock.
/// </summary>
public sealed class EnforcementLifecycleWorkflowState
{
    public Guid ViolationId { get; init; }
    public Guid SubjectId { get; init; }
    public Guid RuleId { get; init; }
    public string Severity { get; init; } = string.Empty;
    public string RecommendedAction { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
    public Guid SanctionId { get; init; }
    public Guid RestrictionId { get; init; }
    public Guid LockId { get; init; }
    public DateTimeOffset DetectedAt { get; init; }
    public string CurrentStep { get; set; } = string.Empty;
}
