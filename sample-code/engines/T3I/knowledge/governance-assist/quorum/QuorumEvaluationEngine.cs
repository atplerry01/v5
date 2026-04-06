namespace Whycespace.Engines.T3I.GovernanceAssist.Quorum;

/// <summary>
/// T3I engine for quorum evaluation.
/// Computes vote tallies, threshold checks, and approval/rejection decisions.
/// Stateless — all inputs provided per invocation.
/// </summary>
public sealed class QuorumEvaluationEngine
{
    public QuorumResult Evaluate(IReadOnlyList<GuardianVote> votes, QuorumThreshold threshold)
    {
        var approvals = votes.Count(v => v.Decision == VoteDecision.Approve);
        var rejections = votes.Count(v => v.Decision == VoteDecision.Reject);
        var total = votes.Count;

        var quorumMet = total >= threshold.MinimumVoters;
        var approved = quorumMet && approvals >= threshold.RequiredApprovals;
        var rejected = quorumMet && rejections > (total - threshold.RequiredApprovals);

        return new QuorumResult
        {
            QuorumMet = quorumMet,
            Approved = approved,
            Rejected = rejected,
            Pending = !approved && !rejected,
            Approvals = approvals,
            Rejections = rejections,
            TotalVotes = total,
            RequiredApprovals = threshold.RequiredApprovals,
            RequiredVoters = threshold.MinimumVoters
        };
    }
}

public sealed record GuardianVote
{
    public required string GuardianId { get; init; }
    public required VoteDecision Decision { get; init; }
    public string? Reason { get; init; }
    public required DateTime CastAt { get; init; }
}

public enum VoteDecision
{
    Approve,
    Reject,
    Abstain
}

public sealed record QuorumThreshold
{
    public required int MinimumVoters { get; init; }
    public required int RequiredApprovals { get; init; }
}

public sealed record QuorumResult
{
    public required bool QuorumMet { get; init; }
    public required bool Approved { get; init; }
    public required bool Rejected { get; init; }
    public required bool Pending { get; init; }
    public required int Approvals { get; init; }
    public required int Rejections { get; init; }
    public required int TotalVotes { get; init; }
    public required int RequiredApprovals { get; init; }
    public required int RequiredVoters { get; init; }
}
