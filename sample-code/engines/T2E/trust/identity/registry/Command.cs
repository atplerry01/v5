namespace Whycespace.Engines.T2E.Trust.Identity.Registry;

public record IdentityRegistryCommand(
    string Action,
    string EntityId,
    object Payload
);
