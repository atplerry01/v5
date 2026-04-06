namespace Whycespace.Engines.T2E.Constitutional.Policy.Enforcement;

public record PolicyEnforcementCommand(string Action, string EntityId, object Payload);
public sealed record CreatePolicyEnforcementCommand(string Id) : PolicyEnforcementCommand("Create", Id, null!);
