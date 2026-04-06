namespace Whycespace.Shared.Contracts.Policy;

/// <summary>
/// Standardized result from any policy guard across all systems.
/// </summary>
public sealed record PolicyGuardResult
{
    public required bool IsAllowed { get; init; }
    public string? Reason { get; init; }
    public IReadOnlyList<string> Violations { get; init; } = [];
    public PolicyEventData? EventPayload { get; init; }

    public bool IsDenied => !IsAllowed;

    public static PolicyGuardResult Allowed(PolicyEventData? eventPayload = null) => new()
    {
        IsAllowed = true,
        EventPayload = eventPayload
    };

    public static PolicyGuardResult Denied(
        string reason,
        IReadOnlyList<string>? violations = null,
        PolicyEventData? eventPayload = null) => new()
    {
        IsAllowed = false,
        Reason = reason,
        Violations = violations ?? [reason],
        EventPayload = eventPayload
    };
}
