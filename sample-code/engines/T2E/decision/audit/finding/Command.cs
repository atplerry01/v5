namespace Whycespace.Engines.T2E.Decision.Audit.Finding;

public record FindingCommand(
    string Action,
    string EntityId,
    object Payload
);
