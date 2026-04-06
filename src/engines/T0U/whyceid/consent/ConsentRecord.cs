namespace Whyce.Engines.T0U.WhyceId.Consent;

/// <summary>
/// Immutable consent record. Tracks what an identity has consented to.
/// </summary>
public sealed record ConsentRecord(
    string ConsentId,
    string ConsentType,
    string IdentityId,
    bool IsGranted,
    string ConsentHash);
