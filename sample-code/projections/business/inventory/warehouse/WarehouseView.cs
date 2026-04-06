namespace Whycespace.Projections.Business.Inventory.Warehouse;

public sealed record WarehouseView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
