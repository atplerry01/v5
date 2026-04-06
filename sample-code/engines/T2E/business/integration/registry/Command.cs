namespace Whycespace.Engines.T2E.Business.Integration.Registry;

public record RegistryCommand(
    string Action,
    string EntityId,
    object Payload
);
