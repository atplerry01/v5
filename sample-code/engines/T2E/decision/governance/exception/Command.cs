namespace Whycespace.Engines.T2E.Decision.Governance.Exception;

public record ExceptionCommand(
    string Action,
    string EntityId,
    object Payload
);
