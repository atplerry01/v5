namespace Whycespace.Engines.T2E.Decision.Risk.Exception;

public record ExceptionCommand(
    string Action,
    string EntityId,
    object Payload
);
