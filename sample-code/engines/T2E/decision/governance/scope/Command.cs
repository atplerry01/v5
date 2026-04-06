namespace Whycespace.Engines.T2E.Decision.Governance.Scope;

public record ScopeCommand(
    string Action,
    string EntityId,
    object Payload
);
