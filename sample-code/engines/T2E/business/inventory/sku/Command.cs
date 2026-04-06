namespace Whycespace.Engines.T2E.Business.Inventory.Sku;

public record SkuCommand(
    string Action,
    string EntityId,
    object Payload
);
