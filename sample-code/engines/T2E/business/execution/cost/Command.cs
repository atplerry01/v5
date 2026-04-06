namespace Whycespace.Engines.T2E.Business.Execution.Cost;

public record CostCommand(
    string Action,
    string EntityId,
    object Payload
);
