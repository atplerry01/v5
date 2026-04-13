namespace Whycespace.Engines.T0U.WhycePolicy.Safeguard;

/// <summary>
/// Constitutional safeguard. Non-bypassable policy rules that protect system invariants.
/// Safeguards are always evaluated first, before any other policy rules.
/// </summary>
public sealed record PolicySafeguard(
    string SafeguardId,
    string SafeguardName,
    string Description,
    string SafeguardHash)
{
    /// <summary>
    /// Safeguard evaluation. Returns false if the safeguard is violated.
    /// </summary>
    public bool Evaluate(string identityId, int trustScore)
    {
        // Constitutional: identity MUST exist
        if (string.IsNullOrEmpty(identityId))
            return false;

        // Constitutional: trust score cannot be negative
        if (trustScore < 0)
            return false;

        return true;
    }
}
