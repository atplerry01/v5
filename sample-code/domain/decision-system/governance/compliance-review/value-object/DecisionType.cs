namespace Whycespace.Domain.DecisionSystem.Governance.ComplianceReview;

public sealed record DecisionType(string Value)
{
    public static readonly DecisionType ProposalApproval = new("ProposalApproval");
    public static readonly DecisionType ProposalRejection = new("ProposalRejection");
    public static readonly DecisionType BallotClosure = new("BallotClosure");
    public static readonly DecisionType QuorumReached = new("QuorumReached");
    public static readonly DecisionType DelegationChange = new("DelegationChange");

    public override string ToString() => Value;
}
