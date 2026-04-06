namespace Whycespace.Engines.T2E.Constitutional.Policy.Version;

public record PolicyVersionCommand(
    string Action,
    string EntityId,
    object Payload
);
