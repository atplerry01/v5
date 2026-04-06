namespace Whycespace.Projections.Business.Inventory.Sku;

public sealed record SkuView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
