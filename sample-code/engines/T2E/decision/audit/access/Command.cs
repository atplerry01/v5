namespace Whycespace.Engines.T2E.Decision.Audit.Access;

public record AccessCommand(
    string Action,
    string EntityId,
    object Payload
);
