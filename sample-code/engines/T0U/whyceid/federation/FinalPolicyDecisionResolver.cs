using Whycespace.Engines.T0U.WhyceId.Federation;

namespace Whycespace.Engines.T0U.WhyceId.Federation;

/// <summary>
/// FIX 1 — Resolves final policy decision by merging WHYCEPOLICY + FederationPolicyEnforcer.
///
/// Precedence: WHYCEPOLICY OVERRIDES ALL.
///
///   if WHYCEPOLICY = DENY        → final = DENY  (regardless of federation decision)
///   if WHYCEPOLICY = CONDITIONAL → final = CONDITIONAL
///   if WHYCEPOLICY = ALLOW       → use FederationPolicyEnforcer result
///
/// Deterministic: same inputs always produce same output.
/// </summary>
public static class FinalPolicyDecisionResolver
{
    public static FinalPolicyDecision Resolve(
        string? whycePolicyOutcome,
        string? whycePolicyReason,
        FederationPolicyDecision federationDecision)
    {
        ArgumentNullException.ThrowIfNull(federationDecision);

        // WHYCEPOLICY overrides all
        if (whycePolicyOutcome is not null)
        {
            if (whycePolicyOutcome == "Deny")
                return FinalPolicyDecision.Deny(
                    $"WHYCEPOLICY DENY: {whycePolicyReason ?? "Policy denied"}",
                    "whycepolicy");

            if (whycePolicyOutcome == "Conditional")
                return FinalPolicyDecision.Conditional(
                    $"WHYCEPOLICY CONDITIONAL: {whycePolicyReason ?? "Step-up required"}",
                    "whycepolicy");

            // WHYCEPOLICY = Allow → fall through to federation decision
        }

        // Use federation enforcer result
        return new FinalPolicyDecision
        {
            Outcome = federationDecision.Outcome,
            Reason = federationDecision.Reason,
            Source = "federation_enforcer",
            WhycePolicyOverridden = false
        };
    }
}

public sealed record FinalPolicyDecision
{
    public required string Outcome { get; init; }
    public string? Reason { get; init; }
    public required string Source { get; init; }
    public bool WhycePolicyOverridden { get; init; }

    public bool IsAllowed => Outcome == "Allow";
    public bool IsDenied => Outcome == "Deny";
    public bool IsConditional => Outcome == "Conditional";

    public static FinalPolicyDecision Deny(string reason, string source) =>
        new() { Outcome = "Deny", Reason = reason, Source = source, WhycePolicyOverridden = true };

    public static FinalPolicyDecision Conditional(string reason, string source) =>
        new() { Outcome = "Conditional", Reason = reason, Source = source, WhycePolicyOverridden = true };
}
