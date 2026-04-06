namespace Whycespace.Engines.T2E.Business.Execution.Sourcing;

public record SourcingCommand(
    string Action,
    string EntityId,
    object Payload
);
