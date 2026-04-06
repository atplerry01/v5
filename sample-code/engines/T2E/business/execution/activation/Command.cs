namespace Whycespace.Engines.T2E.Business.Execution.Activation;

public record ActivationCommand(
    string Action,
    string EntityId,
    object Payload
);
