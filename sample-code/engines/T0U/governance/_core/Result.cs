namespace Whycespace.Engines.T0U.Governance;

public sealed record ProposalResult(string ProposalId, bool IsAccepted);

public sealed record VoteResult(string ProposalId, string VoterId, bool Recorded);

public sealed record QuorumResult(string ProposalId, bool QuorumReached, int VotesCast, int Required);

/// <summary>
/// Generic validation result returned by T0U governance validation engines.
/// T2E checks IsValid before proceeding with execution.
/// </summary>
public sealed record GovernanceValidationResult(bool IsValid, string? Reason = null)
{
    public static GovernanceValidationResult Valid() => new(true);
    public static GovernanceValidationResult Invalid(string reason) => new(false, reason);
}
