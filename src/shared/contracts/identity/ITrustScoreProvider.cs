namespace Whyce.Shared.Contracts.Identity;

/// <summary>
/// Contract for external trust score enrichment.
/// Allows integration with external trust/reputation systems.
/// </summary>
public interface ITrustScoreProvider
{
    Task<TrustScoreEnrichment> EnrichAsync(string identityId, int baseScore);
}

public sealed record TrustScoreEnrichment(
    int AdjustedScore,
    string[] AdditionalFactors,
    string EnrichmentHash);
