namespace Whycespace.Engines.T2E.Decision.Governance.Charter;

public record CharterCommand(
    string Action,
    string EntityId,
    object Payload
);
