namespace Whycespace.Engines.T2E.Business.Execution.Completion;

public record CompletionCommand(
    string Action,
    string EntityId,
    object Payload
);
