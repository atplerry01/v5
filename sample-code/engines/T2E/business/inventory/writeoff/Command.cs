namespace Whycespace.Engines.T2E.Business.Inventory.Writeoff;

public record WriteoffCommand(
    string Action,
    string EntityId,
    object Payload
);
