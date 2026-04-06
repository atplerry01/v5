namespace Whycespace.Projections.Intelligence.Capacity.Demand;

public sealed record DemandView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
