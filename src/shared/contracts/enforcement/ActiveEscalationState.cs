namespace Whycespace.Shared.Contracts.Enforcement;

/// <summary>
/// Snapshot of a subject's current escalation level, consumed by runtime
/// middleware to progressively restrict command execution. Sanction decisions
/// (what each level actually *does*) remain owned by WHYCEPOLICY; this record
/// only surfaces the classifier output from the escalation projection.
///
/// Level ordering (lowest → highest): None, Low, Medium, High, Critical.
/// Runtime constraint semantics:
///   • None/Low     — allow; log only.
///   • Medium       — stamp EnforcementConstraint = "Medium"; allow.
///   • High         — stamp EnforcementConstraint = "High"; allow (handlers may degrade).
///   • Critical     — reject command outright.
/// </summary>
public sealed record ActiveEscalationState(string Level, int Count, int SeverityScore)
{
    public static readonly ActiveEscalationState None = new("None", 0, 0);

    public bool IsCritical => string.Equals(Level, "Critical", StringComparison.Ordinal);
    public bool IsHigh     => string.Equals(Level, "High", StringComparison.Ordinal);
    public bool IsMedium   => string.Equals(Level, "Medium", StringComparison.Ordinal);
}
