namespace Whycespace.Projections.Economic;

/// <summary>
/// Active enforcement state per identity.
/// Sourced from EnforcementAppliedEvent / EnforcementReleasedEvent.
/// Global ordering: events applied only if newer than current state.
/// </summary>
public sealed record EnforcementReadModel
{
    public required string IdentityId { get; init; }
    public required string EnforcementType { get; init; }
    public required string Reason { get; init; }
    public required bool IsActive { get; init; }
    public DateTimeOffset AppliedAt { get; init; }
    public DateTimeOffset? ReleasedAt { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
    public DateTimeOffset LastEventTimestamp { get; init; }
    public long LastEventVersion { get; init; }
}
