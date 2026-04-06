namespace Whycespace.Shared.Contracts.Governance;

/// <summary>
/// Shared governance contracts — pure data types used across systems and engines.
/// These types carry no business logic, only structure.
/// Engines provide implementations; systems consume via intent dispatch.
/// </summary>

public enum VoteDecision
{
    Approve,
    Reject,
    Abstain
}

public sealed record GuardianVote
{
    public required string GuardianId { get; init; }
    public required VoteDecision Decision { get; init; }
    public string? Reason { get; init; }
    public required DateTime CastAt { get; init; }
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

public enum ApprovalOutcome
{
    Approved,
    Rejected,
    Pending,
    QuorumNotMet
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

/// <summary>
/// Contract for tier-0 action classification.
/// Tier-0 actions require guardian approval before execution.
/// </summary>
public static class Tier0ActionClassifier
{
    public static bool IsTier0Action(string commandType) =>
        commandType.StartsWith("policy.modify", StringComparison.OrdinalIgnoreCase) ||
        commandType.StartsWith("system.config", StringComparison.OrdinalIgnoreCase) ||
        commandType.StartsWith("economic.rule", StringComparison.OrdinalIgnoreCase) ||
        commandType.StartsWith("governance.override", StringComparison.OrdinalIgnoreCase);
}
