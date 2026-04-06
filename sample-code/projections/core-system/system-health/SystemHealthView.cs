namespace Whycespace.Projections.CoreSystem.SystemHealth;

public sealed record SystemHealthView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public required bool IsAuthoritative { get; init; }
    public required bool IsDegraded { get; init; }
    public string? SnapshotHash { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
