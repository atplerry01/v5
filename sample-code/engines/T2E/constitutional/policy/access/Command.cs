namespace Whycespace.Engines.T2E.Constitutional.Policy.Access;

public record PolicyAccessCommand(
    string Action,
    string EntityId,
    object Payload
);
