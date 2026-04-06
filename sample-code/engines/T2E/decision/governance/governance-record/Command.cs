namespace Whycespace.Engines.T2E.Decision.Governance.GovernanceRecord;

public record GovernanceRecordCommand(
    string Action,
    string EntityId,
    object Payload
);
