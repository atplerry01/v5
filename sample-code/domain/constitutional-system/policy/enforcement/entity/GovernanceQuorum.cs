namespace Whycespace.Domain.ConstitutionalSystem.Policy.Registry;

public sealed class GovernanceQuorum
{
    public int RequiredApprovals { get; }
    public IReadOnlyList<ApproverRole> RolesRequired { get; }
    public ApprovalStrategy Strategy { get; }

    private GovernanceQuorum(int requiredApprovals, IReadOnlyList<ApproverRole> rolesRequired, ApprovalStrategy strategy)
    {
        RequiredApprovals = requiredApprovals;
        RolesRequired = rolesRequired;
        Strategy = strategy;
    }

    public static GovernanceQuorum Create(
        int requiredApprovals,
        IReadOnlyList<ApproverRole> rolesRequired,
        ApprovalStrategy? strategy = null)
    {
        if (requiredApprovals < 1)
            throw new ArgumentOutOfRangeException(nameof(requiredApprovals));

        return new GovernanceQuorum(requiredApprovals, rolesRequired, strategy ?? ApprovalStrategy.Majority);
    }

    public bool IsMet(IReadOnlyList<PolicyApproval> approvals)
    {
        if (approvals.Count < RequiredApprovals)
            return false;

        if (Strategy == ApprovalStrategy.All)
            return RolesRequired.All(role => approvals.Any(a => a.Role == role));

        if (Strategy == ApprovalStrategy.Majority)
            return approvals.Count >= RequiredApprovals;

        return approvals.Count >= RequiredApprovals;
    }
}
