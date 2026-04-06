namespace Whycespace.Domain.IntelligenceSystem.Identity.IdentityIntelligence;

/// <summary>
/// Immutable scoring version identifier. Every intelligence computation
/// is tagged with the version that produced it.
///
/// VersionId: human-readable label (e.g. "v1.0")
/// ConfigHash: SHA256 of the serialized scoring config — guarantees no silent changes
/// ActivatedAt: when this version became active
/// </summary>
public sealed record ScoringVersion(
    string VersionId,
    string ConfigHash,
    DateTimeOffset ActivatedAt)
{
    public override string ToString() => $"{VersionId} ({ConfigHash[..8]})";
}
