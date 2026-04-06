namespace Whycespace.Shared.Contracts.Policy;

/// <summary>
/// Intelligence context exposed to policy evaluation.
/// Trust/Risk scores influence authorization and policy decisions.
///
/// Policy enforcement rules:
///   if RiskScore > RiskDenyThreshold     → DENY
///   if TrustScore &lt; TrustConditionalMin → CONDITIONAL (step-up auth required)
///   if HasActiveAnomalies                → flag for review
///   if GamingDetected                    → escalate
///
/// Intelligence outputs are advisory unless enforced by policy rules.
/// </summary>
public sealed record IntelligencePolicyContext
{
    public required decimal TrustScore { get; init; }
    public required string TrustClassification { get; init; }
    public required decimal RiskScore { get; init; }
    public required string RiskSeverity { get; init; }
    public required bool HasActiveAnomalies { get; init; }
    public required int AnomalyCount { get; init; }
    public required IReadOnlyList<string> AnomalyFlags { get; init; }
    public required bool GamingDetected { get; init; }

    /// <summary>
    /// Evaluate policy enforcement decision from intelligence context.
    /// Deterministic: same context always produces same decision.
    /// </summary>
    public PolicyEnforcementDecision Evaluate(
        decimal riskDenyThreshold = 80m,
        decimal trustConditionalMin = 30m)
    {
        if (RiskScore >= riskDenyThreshold)
            return PolicyEnforcementDecision.Deny($"Risk score {RiskScore:F1} exceeds threshold {riskDenyThreshold}");

        if (TrustScore < trustConditionalMin)
            return PolicyEnforcementDecision.Conditional($"Trust score {TrustScore:F1} below minimum {trustConditionalMin}");

        if (GamingDetected)
            return PolicyEnforcementDecision.Conditional("Behavior gaming detected — step-up auth required");

        if (HasActiveAnomalies)
            return PolicyEnforcementDecision.AllowWithFlag($"{AnomalyCount} active anomalies flagged for review");

        return PolicyEnforcementDecision.Allow();
    }

    public static IntelligencePolicyContext Empty => new()
    {
        TrustScore = 0m,
        TrustClassification = "Unknown",
        RiskScore = 0m,
        RiskSeverity = "Unknown",
        HasActiveAnomalies = false,
        AnomalyCount = 0,
        AnomalyFlags = [],
        GamingDetected = false
    };
}

public sealed record PolicyEnforcementDecision
{
    public required string Outcome { get; init; }
    public string? Reason { get; init; }

    public bool IsAllowed => Outcome is "allow" or "allow_with_flag";
    public bool IsDenied => Outcome == "deny";
    public bool IsConditional => Outcome == "conditional";

    public static PolicyEnforcementDecision Allow() => new() { Outcome = "allow" };
    public static PolicyEnforcementDecision AllowWithFlag(string reason) => new() { Outcome = "allow_with_flag", Reason = reason };
    public static PolicyEnforcementDecision Conditional(string reason) => new() { Outcome = "conditional", Reason = reason };
    public static PolicyEnforcementDecision Deny(string reason) => new() { Outcome = "deny", Reason = reason };
}
