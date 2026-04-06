namespace Whycespace.Domain.IntelligenceSystem.Identity.IdentityIntelligence;

/// <summary>
/// A computed behavioral pattern signature for anomaly comparison.
/// Deterministic: same input events produce the same signature.
/// </summary>
public sealed record PatternSignature(
    string SignatureHash,
    int EventCount,
    decimal AverageInterval,
    string DominantEventType);
