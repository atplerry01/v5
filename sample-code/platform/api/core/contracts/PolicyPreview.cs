namespace Whycespace.Platform.Api.Core.Contracts;

/// <summary>
/// Result of a policy pre-check evaluation.
/// Provides user-visible, explainable feedback BEFORE execution begins.
/// Platform surfaces this — it does NOT evaluate policy logic.
///
/// ADVISORY ONLY — does NOT block execution.
/// Runtime WhycePolicy is the final authority.
///
/// Explainability:
/// - Decision: ALLOW / DENY / CONDITIONAL
/// - Reasons: human-readable explanations
/// - Violations: machine-readable violation codes
/// - PolicyId: which policy produced this decision (if available)
/// - Metadata: structural context carried through
/// </summary>
public sealed record PolicyPreview
{
    public required string Decision { get; init; }
    public IReadOnlyList<string> Reasons { get; init; } = [];
    public IReadOnlyList<string> Violations { get; init; } = [];
    public string? PolicyId { get; init; }
    public IReadOnlyDictionary<string, string>? Metadata { get; init; }

    public bool IsAllowed => string.Equals(Decision, "ALLOW", StringComparison.OrdinalIgnoreCase);
    public bool IsDenied => string.Equals(Decision, "DENY", StringComparison.OrdinalIgnoreCase);
    public bool IsConditional => string.Equals(Decision, "CONDITIONAL", StringComparison.OrdinalIgnoreCase);

    public static PolicyPreview Allow(
        string? reason = null,
        string? policyId = null,
        IReadOnlyDictionary<string, string>? metadata = null) => new()
    {
        Decision = "ALLOW",
        Reasons = reason is not null ? [reason] : ["Policy check passed"],
        PolicyId = policyId,
        Metadata = metadata
    };

    public static PolicyPreview Deny(
        IReadOnlyList<string> reasons,
        IReadOnlyList<string>? violations = null,
        string? policyId = null,
        IReadOnlyDictionary<string, string>? metadata = null) => new()
    {
        Decision = "DENY",
        Reasons = reasons,
        Violations = violations ?? [],
        PolicyId = policyId,
        Metadata = metadata
    };

    public static PolicyPreview Deny(
        string reason,
        string? violation = null,
        string? policyId = null) => new()
    {
        Decision = "DENY",
        Reasons = [reason],
        Violations = violation is not null ? [violation] : [],
        PolicyId = policyId
    };

    public static PolicyPreview Conditional(
        IReadOnlyList<string> reasons,
        IReadOnlyList<string>? violations = null,
        string? policyId = null,
        IReadOnlyDictionary<string, string>? metadata = null) => new()
    {
        Decision = "CONDITIONAL",
        Reasons = reasons,
        Violations = violations ?? [],
        PolicyId = policyId,
        Metadata = metadata
    };

    public static PolicyPreview Conditional(
        string reason,
        string? policyId = null) => new()
    {
        Decision = "CONDITIONAL",
        Reasons = [reason],
        PolicyId = policyId
    };

    /// <summary>
    /// Fallback when the preview service itself fails.
    /// Returns ALLOW with a note — preview failure must never block execution.
    /// </summary>
    public static PolicyPreview ServiceUnavailable() => new()
    {
        Decision = "ALLOW",
        Reasons = ["Policy preview service unavailable — defaulting to ALLOW (advisory only)"],
        Metadata = new Dictionary<string, string> { ["fallback"] = "true" }
    };
}
