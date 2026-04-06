namespace Whycespace.Engines.T2E.Decision.Governance.Guardian;

public record GuardianCommand(
    string Action,
    string EntityId,
    object Payload
);
