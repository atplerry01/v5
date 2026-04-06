namespace Whycespace.Platform.Api.Core.Contracts;

/// <summary>
/// Resolved identity profile from WhyceID (T0U).
/// Immutable, read-only view of identity state.
/// Platform does NOT mutate or store this — it is resolved per-request
/// by the IWhyceIdService adapter and carried through the pipeline via headers.
/// </summary>
public sealed record WhyceIdentity
{
    public required Guid IdentityId { get; init; }
    public required IReadOnlyList<string> Roles { get; init; }
    public required IReadOnlyDictionary<string, string> Attributes { get; init; }
    public required decimal TrustScore { get; init; }
    public required IReadOnlyList<string> Consents { get; init; }
    public required bool IsVerified { get; init; }
    public string? SessionId { get; init; }
}
