namespace Whycespace.Projections.Business.Portfolio.Rebalance;

public sealed record RebalanceView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
