namespace Whycespace.Domain.DecisionSystem.Governance.Proposal;

public sealed record ProposalStatus(string Value)
{
    public static readonly ProposalStatus Pending = new("Pending");
    public static readonly ProposalStatus Approved = new("Approved");
    public static readonly ProposalStatus Rejected = new("Rejected");

    public bool IsTerminal => this == Approved || this == Rejected;

    public override string ToString() => Value;
}
