namespace Whycespace.Engines.T2E.Decision.Governance.Appeal;

public record AppealCommand(
    string Action,
    string EntityId,
    object Payload
);
