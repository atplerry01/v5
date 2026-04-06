namespace Whycespace.Engines.T3I.IdentityFederation;

/// <summary>
/// T3I Guard — controls how federated trust influences identity trust (E9 integration).
///
/// Rules:
///   - Cap maximum trust contribution from any single federation source
///   - Block propagation if issuer is degraded or suspicious
///   - Require minimum confidence before propagating trust
///
/// Stateless. No persistence. Deterministic.
/// </summary>
public sealed class FederationTrustPropagationGuard
{
    public TrustPropagationResult Evaluate(TrustPropagationInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        // Block: issuer reputation is suspicious
        if (input.IssuerReputationStatus == "Suspicious")
            return TrustPropagationResult.Blocked(
                $"Issuer reputation is '{input.IssuerReputationStatus}' — trust propagation blocked.");

        // Block: issuer trust is degraded
        if (input.IssuerTrustStatus == "Degraded" || input.IssuerTrustStatus == "Suspended")
            return TrustPropagationResult.Blocked(
                $"Issuer trust status is '{input.IssuerTrustStatus}' — trust propagation blocked.");

        // Block: confidence below minimum
        if (input.LinkConfidence < input.MinConfidenceForPropagation)
            return TrustPropagationResult.Blocked(
                $"Link confidence {input.LinkConfidence:F2} is below minimum {input.MinConfidenceForPropagation} for propagation.");

        // Compute capped contribution
        var rawContribution = input.NormalizedTrustScore * input.LinkConfidence;
        var cappedContribution = Math.Min(rawContribution, input.MaxTrustContribution);

        return TrustPropagationResult.Allowed(cappedContribution);
    }
}

public sealed record TrustPropagationInput
{
    public required decimal NormalizedTrustScore { get; init; }
    public required decimal LinkConfidence { get; init; }
    public required string IssuerReputationStatus { get; init; }
    public required string IssuerTrustStatus { get; init; }
    public required decimal MaxTrustContribution { get; init; }
    public required decimal MinConfidenceForPropagation { get; init; }
}
