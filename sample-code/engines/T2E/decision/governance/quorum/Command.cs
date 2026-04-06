namespace Whycespace.Engines.T2E.Decision.Governance.Quorum;

public record QuorumCommand(string Action, string EntityId, object Payload);
public sealed record CreateQuorumCommand(string Id) : QuorumCommand("Create", Id, null!);
