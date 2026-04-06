namespace Whycespace.Platform.Api.Core.Contracts.Access;

/// <summary>
/// Read-only identity access dashboard view.
/// Exposes RBAC roles, ABAC attributes, TrustScore, and consents.
/// Sourced from WhyceID projections — no access logic computation.
///
/// Does NOT expose: raw policy rules, internal scoring algorithms,
/// system-sensitive attributes. Only safe, user-visible access state.
/// </summary>
public sealed record IdentityAccessView
{
    public required Guid IdentityId { get; init; }
    public required IReadOnlyList<string> Roles { get; init; }
    public required IReadOnlyDictionary<string, string> Attributes { get; init; }
    public required decimal TrustScore { get; init; }
    public required IReadOnlyList<string> Consents { get; init; }
    public required bool IsVerified { get; init; }
    public string? SessionId { get; init; }
}
