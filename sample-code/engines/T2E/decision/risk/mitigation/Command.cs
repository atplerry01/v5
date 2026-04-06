namespace Whycespace.Engines.T2E.Decision.Risk.Mitigation;

public record MitigationCommand(
    string Action,
    string EntityId,
    object Payload
);
