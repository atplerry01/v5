namespace Whycespace.Engines.T2E.Decision.Compliance.Filing;

public record FilingCommand(
    string Action,
    string EntityId,
    object Payload
);
