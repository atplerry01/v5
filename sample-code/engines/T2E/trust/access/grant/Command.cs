namespace Whycespace.Engines.T2E.Trust.Access.Grant;

public record GrantCommand(
    string Action,
    string EntityId,
    object Payload
);
