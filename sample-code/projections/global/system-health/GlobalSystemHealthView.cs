namespace Whycespace.Projections.Global.SystemHealth;

public sealed record GlobalSystemHealthView
{
    public required string Id { get; init; }
    public required string RegionId { get; init; }
    public required string SystemName { get; init; }
    public required string Status { get; init; }
    public required bool IsHealthy { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
