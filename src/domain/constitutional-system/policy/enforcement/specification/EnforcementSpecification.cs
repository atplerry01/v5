namespace Whycespace.Domain.ConstitutionalSystem.Policy.Enforcement;

public sealed class CanApplySpecification
{
    public bool IsSatisfiedBy(EnforcementStatus status)
    {
        return status == EnforcementStatus.Pending;
    }
}

public sealed class CanWithdrawSpecification
{
    public bool IsSatisfiedBy(EnforcementStatus status)
    {
        return status == EnforcementStatus.Applied;
    }
}
