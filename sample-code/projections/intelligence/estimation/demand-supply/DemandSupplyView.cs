namespace Whycespace.Projections.Intelligence.Estimation.DemandSupply;

public sealed record DemandSupplyView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
