namespace Whycespace.Engines.T2E.Business.Inventory.Valuation;

public record ValuationCommand(
    string Action,
    string EntityId,
    object Payload
);
