namespace Whycespace.Engines.T2E.Business.Inventory.Replenishment;

public record ReplenishmentCommand(
    string Action,
    string EntityId,
    object Payload
);
