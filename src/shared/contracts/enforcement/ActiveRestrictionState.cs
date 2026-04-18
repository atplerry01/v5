namespace Whycespace.Shared.Contracts.Enforcement;

/// <summary>
/// Snapshot of the currently-active enforcement restriction posture for a
/// subject. Produced by <see cref="IRestrictionStateQuery"/> from the
/// restriction read model and consumed by runtime middleware to block
/// command execution when an active restriction exists.
///
/// Any active restriction with a matching scope blocks the subject's
/// commands. Absence of any active restriction yields <see cref="None"/>.
/// </summary>
public sealed record ActiveRestrictionState(bool IsRestricted, string? Scope, string? Reason)
{
    public static readonly ActiveRestrictionState None = new(false, null, null);
}
