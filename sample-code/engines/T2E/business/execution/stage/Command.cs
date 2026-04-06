namespace Whycespace.Engines.T2E.Business.Execution.Stage;

public record StageCommand(
    string Action,
    string EntityId,
    object Payload
);
