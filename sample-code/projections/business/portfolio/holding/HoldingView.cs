namespace Whycespace.Projections.Business.Portfolio.Holding;

public sealed record HoldingView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
