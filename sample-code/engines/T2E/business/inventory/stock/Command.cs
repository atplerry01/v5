namespace Whycespace.Engines.T2E.Business.Inventory.Stock;

public record StockCommand(
    string Action,
    string EntityId,
    object Payload
);
