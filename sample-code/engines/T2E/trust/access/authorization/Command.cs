namespace Whycespace.Engines.T2E.Trust.Access.Authorization;

public record AuthorizationCommand(
    string Action,
    string EntityId,
    object Payload
);
