namespace Whycespace.Engines.T2E.Business.Agreement.Approval;

public record ApprovalCommand(
    string Action,
    string EntityId,
    object Payload
);
