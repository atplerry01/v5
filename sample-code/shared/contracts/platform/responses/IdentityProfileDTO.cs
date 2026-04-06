namespace Whycespace.Shared.Contracts.Platform.Responses;

/// <summary>
/// Flattened, version-aware response DTO for identity profile information.
/// No domain entities exposed — primitives only.
/// </summary>
public sealed record IdentityProfileDTO
{
    public required int Version { get; init; }

    public required Guid IdentityId { get; init; }
    public required string Status { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }

    public string? JurisdictionId { get; init; }
    public int? TrustLevel { get; init; }
    public string[]? Roles { get; init; }
    public string[]? Permissions { get; init; }
    public string? VerificationStatus { get; init; }
}
