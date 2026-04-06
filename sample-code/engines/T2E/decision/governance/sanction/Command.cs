namespace Whycespace.Engines.T2E.Decision.Governance.Sanction;

public record SanctionCommand(
    string Action,
    string EntityId,
    object Payload
);
