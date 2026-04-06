namespace Whycespace.Engines.T0U.Governance;

public abstract record GovernanceCommand;

public sealed record SubmitProposalCommand(string ProposalId, string Title, string Description) : GovernanceCommand;

public sealed record CastVoteCommand(string ProposalId, string VoterId, bool Approve) : GovernanceCommand;

public sealed record CheckQuorumCommand(string ProposalId) : GovernanceCommand;

// Validation commands — T0U decides whether T2E may proceed
public sealed record ValidateDelegationCommand(string DelegationId) : GovernanceCommand;

public sealed record ValidateProposalCommand(string ProposalId) : GovernanceCommand;

public sealed record ValidateQuorumCommand(string QuorumId) : GovernanceCommand;

public sealed record ValidateVotingCommand(string VotingId) : GovernanceCommand;
