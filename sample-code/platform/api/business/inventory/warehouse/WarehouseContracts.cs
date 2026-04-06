namespace Whycespace.Platform.Api.Business.Inventory.Warehouse;

public sealed record WarehouseRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record WarehouseResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
