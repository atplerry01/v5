namespace Whycespace.Projections.Structural.Humancapital.Incentive;

public sealed record IncentiveView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
