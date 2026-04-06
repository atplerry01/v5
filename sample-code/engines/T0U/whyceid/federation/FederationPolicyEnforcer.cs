namespace Whycespace.Engines.T0U.WhyceId.Federation;

/// <summary>
/// T0U Constitutional Engine — policy-driven federation enforcement.
///
/// ALL thresholds come from FederationPolicyThresholds input (from WHYCEPOLICY).
/// NO hardcoded constants. Decisions are deterministic for same inputs.
///
/// Stateless. No persistence. Deterministic.
/// Uses string-based types instead of domain enums.
/// </summary>
public sealed class FederationPolicyEnforcer
{
    public FederationPolicyDecision Enforce(EnforceFederationPolicyCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(command.Thresholds);

        var t = command.Thresholds;

        // Hard deny: trust below policy threshold
        if (command.TrustLevel < t.MinTrust)
            return FederationPolicyDecision.Deny(
                $"Issuer trust level {command.TrustLevel:F1} is below policy minimum {t.MinTrust}.");

        // Hard deny: confidence below policy threshold
        if (command.CurrentConfidence < t.MinConfidence)
            return FederationPolicyDecision.Deny(
                $"Confidence {command.CurrentConfidence:F2} is below policy minimum {t.MinConfidence}.");

        // Hard deny: trust suspended (always denied regardless of policy)
        if (command.TrustStatus == FederationTrustStatusValues.Suspended)
            return FederationPolicyDecision.Deny(
                "Issuer trust status is Suspended.");

        // Policy-driven: system-inferred provenance handling
        if (command.Provenance == ProvenanceSourceValues.SystemInferred)
        {
            return ApplyHandling(t.SystemInferredHandling,
                "System-inferred links — policy requires additional verification.");
        }

        // Policy-driven: degraded trust handling
        if (command.TrustStatus == FederationTrustStatusValues.Degraded)
        {
            return ApplyHandling(t.DegradedHandling,
                "Issuer trust is degraded — policy requires caution.");
        }

        return FederationPolicyDecision.Allow();
    }

    private static FederationPolicyDecision ApplyHandling(string handling, string reason) =>
        handling switch
        {
            "Deny" => FederationPolicyDecision.Deny(reason),
            "Conditional" => FederationPolicyDecision.Conditional(reason),
            "Allow" => FederationPolicyDecision.Allow(),
            _ => FederationPolicyDecision.Conditional(reason)
        };
}
