namespace Whycespace.Engines.T2E.Trust.Identity.Federation;

public record FederationCommand(
    string Action,
    string EntityId,
    object Payload
);
