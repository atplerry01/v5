namespace Whycespace.Engines.T2E.Business.Inventory.Warehouse;

public record WarehouseCommand(
    string Action,
    string EntityId,
    object Payload
);
