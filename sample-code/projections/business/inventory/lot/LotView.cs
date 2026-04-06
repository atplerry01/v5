namespace Whycespace.Projections.Business.Inventory.Lot;

public sealed record LotView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
