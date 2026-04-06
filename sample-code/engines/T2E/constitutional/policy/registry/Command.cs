namespace Whycespace.Engines.T2E.Constitutional.Policy.Registry;

public record PolicyRegistryCommand(
    string Action,
    string EntityId,
    object Payload
);
