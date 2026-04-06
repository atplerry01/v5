namespace Whycespace.Engines.T2E.Business.Inventory.Adjustment;

public record AdjustmentCommand(
    string Action,
    string EntityId,
    object Payload
);
