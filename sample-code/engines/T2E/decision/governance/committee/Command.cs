namespace Whycespace.Engines.T2E.Decision.Governance.Committee;

public record CommitteeCommand(
    string Action,
    string EntityId,
    object Payload
);
