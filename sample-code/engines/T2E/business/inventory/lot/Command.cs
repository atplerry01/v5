namespace Whycespace.Engines.T2E.Business.Inventory.Lot;

public record LotCommand(
    string Action,
    string EntityId,
    object Payload
);
