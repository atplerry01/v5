namespace Whycespace.Domain.BusinessSystem.Execution.Charge;

public sealed class CanReverseSpecification
{
    public bool IsSatisfiedBy(ChargeStatus status)
    {
        return status == ChargeStatus.Charged;
    }
}
