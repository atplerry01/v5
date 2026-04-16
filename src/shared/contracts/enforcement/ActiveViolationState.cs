namespace Whycespace.Shared.Contracts.Enforcement;

/// <summary>
/// Snapshot of the currently-active enforcement posture for a subject. Produced
/// by <see cref="IViolationStateQuery"/> from the violation read model and
/// consumed by runtime middleware to block or constrain execution.
///
/// Priority order: Block overrides Restrict (if both present, IsBlocked==true).
/// Absence of any active violation yields <see cref="None"/>.
/// </summary>
public sealed record ActiveViolationState(bool IsBlocked, string? Constraint)
{
    public static readonly ActiveViolationState None = new(false, null);
}
