namespace Whycespace.Projections.Business.Execution.Cost;

public sealed record CostView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
