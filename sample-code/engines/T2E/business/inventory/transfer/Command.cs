namespace Whycespace.Engines.T2E.Business.Inventory.Transfer;

public record TransferCommand(
    string Action,
    string EntityId,
    object Payload
);
