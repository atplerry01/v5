namespace Whycespace.Projections.Intelligence.Cost.CostStructure;

public sealed record CostStructureView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
