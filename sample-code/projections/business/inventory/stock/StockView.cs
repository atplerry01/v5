namespace Whycespace.Projections.Business.Inventory.Stock;

public sealed record StockView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
