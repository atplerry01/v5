namespace Whycespace.Shared.Contracts.Platform.Queries;

/// <summary>
/// Flattened, version-aware query DTO for retrieving an identity profile.
/// No domain entities exposed — primitives only.
/// </summary>
public sealed record GetIdentityQuery
{
    public required int Version { get; init; }
    public required Guid IdentityId { get; init; }
    public bool IncludeRoles { get; init; }
    public bool IncludePermissions { get; init; }
    public string? CorrelationId { get; init; }
}
