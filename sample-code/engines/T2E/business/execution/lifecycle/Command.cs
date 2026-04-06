namespace Whycespace.Engines.T2E.Business.Execution.Lifecycle;

public record LifecycleCommand(
    string Action,
    string EntityId,
    object Payload
);
