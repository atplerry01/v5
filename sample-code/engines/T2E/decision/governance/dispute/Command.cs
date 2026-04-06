namespace Whycespace.Engines.T2E.Decision.Governance.Dispute;

public record DisputeCommand(
    string Action,
    string EntityId,
    object Payload
);
