using Whycespace.Engines.T3I.GovernanceAssist.Quorum;

namespace Whycespace.Engines.T1M.Wss.Governance;

/// <summary>
/// T1M workflow handler for guardian approval decisions.
/// Evaluates quorum results and produces a decision — approve, reject, or pending.
/// The systems layer orchestrates; this handler decides.
/// </summary>
public sealed class ApprovalDecisionHandler
{
    private readonly QuorumEvaluationEngine _quorumEngine;

    public ApprovalDecisionHandler(QuorumEvaluationEngine quorumEngine)
    {
        _quorumEngine = quorumEngine;
    }

    /// <summary>
    /// Evaluates votes against threshold and returns a typed decision result.
    /// No side effects — pure decision logic.
    /// </summary>
    public ApprovalDecision Evaluate(
        string proposalId,
        string actionType,
        IReadOnlyList<GuardianVote> votes,
        QuorumThreshold threshold)
    {
        var quorumResult = _quorumEngine.Evaluate(votes, threshold);

        if (!quorumResult.QuorumMet)
        {
            return ApprovalDecision.QuorumNotMet(proposalId, actionType,
                $"Quorum not met: {quorumResult.TotalVotes}/{quorumResult.RequiredVoters} voters.");
        }

        if (quorumResult.Rejected)
        {
            return ApprovalDecision.Rejected(proposalId, actionType,
                $"Proposal '{proposalId}' rejected by guardian vote.");
        }

        if (!quorumResult.Approved)
        {
            return ApprovalDecision.Pending(proposalId, actionType,
                $"Proposal '{proposalId}' pending: {quorumResult.Approvals}/{quorumResult.RequiredApprovals} approvals.");
        }

        return ApprovalDecision.Approved(proposalId, actionType);
    }
}

public sealed record ApprovalDecision
{
    public required ApprovalOutcome Outcome { get; init; }
    public required string ProposalId { get; init; }
    public required string ActionType { get; init; }
    public string? Reason { get; init; }

    public bool IsApproved => Outcome == ApprovalOutcome.Approved;

    public static ApprovalDecision Approved(string proposalId, string actionType) =>
        new() { Outcome = ApprovalOutcome.Approved, ProposalId = proposalId, ActionType = actionType };

    public static ApprovalDecision Rejected(string proposalId, string actionType, string reason) =>
        new() { Outcome = ApprovalOutcome.Rejected, ProposalId = proposalId, ActionType = actionType, Reason = reason };

    public static ApprovalDecision Pending(string proposalId, string actionType, string reason) =>
        new() { Outcome = ApprovalOutcome.Pending, ProposalId = proposalId, ActionType = actionType, Reason = reason };

    public static ApprovalDecision QuorumNotMet(string proposalId, string actionType, string reason) =>
        new() { Outcome = ApprovalOutcome.QuorumNotMet, ProposalId = proposalId, ActionType = actionType, Reason = reason };
}

public enum ApprovalOutcome
{
    Approved,
    Rejected,
    Pending,
    QuorumNotMet
}
