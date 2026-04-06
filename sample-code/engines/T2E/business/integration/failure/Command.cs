namespace Whycespace.Engines.T2E.Business.Integration.Failure;

public record FailureCommand(
    string Action,
    string EntityId,
    object Payload
);
