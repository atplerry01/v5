namespace Whycespace.Domain.BusinessSystem.Execution.Charge;

public sealed class IsChargedSpecification
{
    public bool IsSatisfiedBy(ChargeStatus status)
    {
        return status == ChargeStatus.Charged;
    }
}
