namespace Whycespace.Engines.T2E.Decision.Risk.Alert;

public record AlertCommand(
    string Action,
    string EntityId,
    object Payload
);
