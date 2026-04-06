namespace Whycespace.Domain.DecisionSystem.Governance.ComplianceReview;

public sealed record DecisionOutcome(string Value)
{
    public static readonly DecisionOutcome Approved = new("Approved");
    public static readonly DecisionOutcome Rejected = new("Rejected");
    public static readonly DecisionOutcome Abstained = new("Abstained");
    public static readonly DecisionOutcome Deferred = new("Deferred");

    public override string ToString() => Value;
}
