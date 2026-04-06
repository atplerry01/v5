namespace Whycespace.Domain.DecisionSystem.Governance.ClusterDecision;

public sealed record GovernanceDecisionStatus(string Value)
{
    public static readonly GovernanceDecisionStatus Proposed = new("proposed");
    public static readonly GovernanceDecisionStatus Approved = new("approved");
    public static readonly GovernanceDecisionStatus Executed = new("executed");
    public static readonly GovernanceDecisionStatus Rejected = new("rejected");

    public bool IsTerminal => this == Executed || this == Rejected;
}
