namespace Whycespace.Engines.T2E.Constitutional.Policy.Scope;

public record PolicyScopeCommand(
    string Action,
    string EntityId,
    object Payload
);
