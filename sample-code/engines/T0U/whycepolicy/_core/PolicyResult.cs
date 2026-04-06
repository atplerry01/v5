namespace Whycespace.Engines.T0U.WhycePolicy.Core;

public sealed record PolicyResult(bool IsAllowed, string Reason, IReadOnlyList<string> Violations);

public sealed record PolicyValidationResult(bool IsValid, string? Reason = null)
{
    public static PolicyValidationResult Valid() => new(true);
    public static PolicyValidationResult Invalid(string reason) => new(false, reason);
}

public sealed record PolicyEnforcementResult
{
    public required bool Allowed { get; init; }
    public required string CommandType { get; init; }
    public required bool RequiresGuardianApproval { get; init; }
    public required string Reason { get; init; }
}
