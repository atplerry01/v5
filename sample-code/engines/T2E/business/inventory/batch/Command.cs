namespace Whycespace.Engines.T2E.Business.Inventory.Batch;

public record BatchCommand(
    string Action,
    string EntityId,
    object Payload
);
