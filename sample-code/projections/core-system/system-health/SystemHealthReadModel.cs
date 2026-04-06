namespace Whycespace.Projections.CoreSystem.SystemHealth;

public sealed record SystemHealthReadModel
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public long EventStoreVersion { get; init; }
    public int ActiveAggregates { get; init; }
    public string? SnapshotHash { get; init; }
    public int TotalValidations { get; init; }
    public int FailedValidations { get; init; }
    public bool IsAuthoritative { get; init; }
    public bool IsDegraded { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
    public DateTimeOffset LastEventTimestamp { get; init; }
    public long LastEventVersion { get; init; }
}
