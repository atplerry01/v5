namespace Whycespace.Engines.T2E.Business.Integration.Retry;

public record RetryCommand(
    string Action,
    string EntityId,
    object Payload
);
