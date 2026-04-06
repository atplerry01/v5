namespace Whycespace.Engines.T2E.Business.Inventory.Count;

public record CountCommand(
    string Action,
    string EntityId,
    object Payload
);
