namespace Whycespace.Engines.T0U.WhyceId.Federation;

/// <summary>
/// Results from T0U federation engines (constitutional).
/// </summary>
public sealed record FederationValidationResult
{
    public required bool IsValid { get; init; }
    public required IReadOnlyList<string> Reasons { get; init; }

    public static FederationValidationResult Valid() =>
        new() { IsValid = true, Reasons = [] };

    public static FederationValidationResult Invalid(params string[] reasons) =>
        new() { IsValid = false, Reasons = reasons };
}

public sealed record GovernanceDecision
{
    public required string Action { get; init; }
    public required bool Allowed { get; init; }
    public string? RejectionReason { get; init; }

    public static GovernanceDecision Allow(string action) =>
        new() { Action = action, Allowed = true };

    public static GovernanceDecision Reject(string action, string reason) =>
        new() { Action = action, Allowed = false, RejectionReason = reason };
}

public sealed record FederationPolicyDecision
{
    public required string Outcome { get; init; }
    public string? Reason { get; init; }

    public bool IsAllowed => Outcome == "Allow";
    public bool IsDenied => Outcome == "Deny";
    public bool IsConditional => Outcome == "Conditional";

    public static FederationPolicyDecision Allow() =>
        new() { Outcome = "Allow" };

    public static FederationPolicyDecision Conditional(string reason) =>
        new() { Outcome = "Conditional", Reason = reason };

    public static FederationPolicyDecision Deny(string reason) =>
        new() { Outcome = "Deny", Reason = reason };
}
