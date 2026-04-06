namespace Whycespace.Engines.T2E.Decision.Governance.Approval;

public record ApprovalCommand(
    string Action,
    string EntityId,
    object Payload
);
