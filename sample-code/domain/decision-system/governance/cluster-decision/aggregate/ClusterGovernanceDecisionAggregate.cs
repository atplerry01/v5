using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.DecisionSystem.Governance.ClusterDecision;

/// <summary>
/// E18.7.1 — Cluster Governance Decision Aggregate
///
/// Lifecycle: Proposed → Approved → Executed
///            Proposed → Rejected (terminal)
///
/// All decisions are policy-gated and chain-anchored.
/// Intelligence (T3I) recommends but NEVER overrides policy.
/// </summary>
public sealed class ClusterGovernanceDecisionAggregate : AggregateRoot
{
    public Guid ClusterId { get; private set; }
    public string DecisionType { get; private set; } = string.Empty;
    public GovernanceDecisionStatus Status { get; private set; } = GovernanceDecisionStatus.Proposed;
    public string DecisionHash { get; private set; } = string.Empty;

    public ClusterGovernanceDecisionAggregate() { }

    public static ClusterGovernanceDecisionAggregate Propose(
        Guid id,
        Guid clusterId,
        string decisionType,
        string decisionHash)
    {
        if (id == Guid.Empty)
            throw new ClusterGovernanceException("Decision id required");

        if (clusterId == Guid.Empty)
            throw new ClusterGovernanceException("Cluster id required");

        if (string.IsNullOrWhiteSpace(decisionType))
            throw new ClusterGovernanceException("Decision type required");

        if (string.IsNullOrWhiteSpace(decisionHash))
            throw new ClusterGovernanceException("Decision hash required");

        var aggregate = new ClusterGovernanceDecisionAggregate
        {
            Id = id,
            ClusterId = clusterId,
            DecisionType = decisionType,
            DecisionHash = decisionHash,
            Status = GovernanceDecisionStatus.Proposed
        };

        aggregate.RaiseDomainEvent(new GovernanceDecisionProposedEvent(id, clusterId, decisionType));

        return aggregate;
    }

    public void Approve()
    {
        if (Status != GovernanceDecisionStatus.Proposed)
            throw new ClusterGovernanceException("Invalid transition: only Proposed decisions can be approved");

        Status = GovernanceDecisionStatus.Approved;

        RaiseDomainEvent(new GovernanceDecisionApprovedEvent(Id, ClusterId));
    }

    public void Execute()
    {
        if (Status != GovernanceDecisionStatus.Approved)
            throw new ClusterGovernanceException("Must be approved before execution");

        Status = GovernanceDecisionStatus.Executed;

        RaiseDomainEvent(new GovernanceDecisionExecutedEvent(Id, ClusterId));
    }

    public void Reject(string reason)
    {
        if (Status.IsTerminal)
            throw new ClusterGovernanceException("Cannot reject a terminal decision");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ClusterGovernanceException("Rejection reason required");

        Status = GovernanceDecisionStatus.Rejected;

        RaiseDomainEvent(new GovernanceDecisionRejectedEvent(Id, reason));
    }
}
