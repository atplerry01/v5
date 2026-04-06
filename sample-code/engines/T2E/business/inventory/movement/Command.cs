namespace Whycespace.Engines.T2E.Business.Inventory.Movement;

public record MovementCommand(
    string Action,
    string EntityId,
    object Payload
);
