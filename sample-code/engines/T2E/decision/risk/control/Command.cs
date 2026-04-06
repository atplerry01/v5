namespace Whycespace.Engines.T2E.Decision.Risk.Control;

public record ControlCommand(
    string Action,
    string EntityId,
    object Payload
);
