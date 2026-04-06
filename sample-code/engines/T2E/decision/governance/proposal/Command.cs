namespace Whycespace.Engines.T2E.Decision.Governance.Proposal;

public record ProposalCommand(string Action, string EntityId, object Payload);
public sealed record CreateProposalCommand(string Id) : ProposalCommand("Create", Id, null!);
