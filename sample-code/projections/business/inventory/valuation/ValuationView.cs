namespace Whycespace.Projections.Business.Inventory.Valuation;

public sealed record ValuationView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
