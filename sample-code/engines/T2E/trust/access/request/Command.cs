namespace Whycespace.Engines.T2E.Trust.Access.Request;

public record AccessRequestCommand(
    string Action,
    string EntityId,
    object Payload
);
