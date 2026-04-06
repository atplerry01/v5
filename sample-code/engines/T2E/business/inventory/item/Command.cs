namespace Whycespace.Engines.T2E.Business.Inventory.Item;

public record ItemCommand(
    string Action,
    string EntityId,
    object Payload
);
