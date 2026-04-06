namespace Whycespace.Domain.TrustSystem.Identity.Federation;

/// <summary>
/// Domain service for federation trust evaluation.
/// Deterministic: same inputs always produce the same trust score.
///
/// Rules:
///   - NO randomness
///   - NO external calls
///   - NO persistence logic
///   - Pure domain computation only
/// </summary>
public static class FederationTrustService
{
    /// <summary>
    /// Evaluate the trust relationship for an issuer based on credential outcomes and link activity.
    /// </summary>
    public static void EvaluateTrust(
        FederationTrustAggregate trust,
        FederationTrustInput input,
        DateTimeOffset evaluatedAt)
    {
        Guard.AgainstNull(trust);
        Guard.AgainstNull(input);

        trust.Evaluate(input, evaluatedAt);
    }

    /// <summary>
    /// Create a new trust relationship for a registered issuer.
    /// </summary>
    public static FederationTrustAggregate InitializeTrust(
        FederationIssuerAggregate issuer,
        DateTimeOffset evaluatedAt)
    {
        Guard.AgainstNull(issuer);

        return FederationTrustAggregate.Create(
            issuer.IssuerId,
            issuer.TrustLevel,
            evaluatedAt);
    }
}
