namespace Whycespace.Engines.T2E.Decision.Governance.Resolution;

public record ResolutionCommand(
    string Action,
    string EntityId,
    object Payload
);
