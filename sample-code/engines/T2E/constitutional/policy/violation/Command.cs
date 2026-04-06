namespace Whycespace.Engines.T2E.Constitutional.Policy.Violation;

public record ViolationCommand(
    string Action,
    string EntityId,
    object Payload
);
