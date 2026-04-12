namespace Whycespace.Domain.BusinessSystem.Inventory.Warehouse;

public sealed record WarehouseCreatedEvent(WarehouseId WarehouseId, WarehouseCapacity Capacity);
