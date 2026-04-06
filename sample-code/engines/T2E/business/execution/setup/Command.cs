namespace Whycespace.Engines.T2E.Business.Execution.Setup;

public record SetupCommand(
    string Action,
    string EntityId,
    object Payload
);
