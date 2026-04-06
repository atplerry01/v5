namespace Whycespace.Shared.Contracts.Domain.Identity;

/// <summary>
/// Flattened, version-aware DTO for cross-context identity event communication.
/// No domain entity references — only primitive, serializable fields.
/// </summary>
public sealed record IdentityEventDTO : IIdentityEventContract
{
    public required Guid EventId { get; init; }
    public required string EventType { get; init; }
    public required DateTimeOffset OccurredAt { get; init; }
    public required int Version { get; init; }
    public required string CorrelationId { get; init; }

    public required Guid IdentityId { get; init; }
    public required string ActionType { get; init; }
    public string? Reason { get; init; }

    public string? CausationId { get; init; }
    public string? PartitionKey { get; init; }
    public string? IdempotencyKey { get; init; }
    public string? PermissionScope { get; init; }
    public string? RoleName { get; init; }
    public int? TrustLevel { get; init; }
}