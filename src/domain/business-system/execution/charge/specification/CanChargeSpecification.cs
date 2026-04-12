namespace Whycespace.Domain.BusinessSystem.Execution.Charge;

public sealed class CanChargeSpecification
{
    public bool IsSatisfiedBy(ChargeStatus status)
    {
        return status == ChargeStatus.Pending;
    }
}
