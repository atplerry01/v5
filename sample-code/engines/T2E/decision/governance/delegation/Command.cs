namespace Whycespace.Engines.T2E.Decision.Governance.Delegation;

public record DelegationCommand(string Action, string EntityId, object Payload);
public sealed record CreateDelegationCommand(string Id) : DelegationCommand("Create", Id, null!);
