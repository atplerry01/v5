namespace Whycespace.Domain.BusinessSystem.Portfolio.Mandate;

public sealed class CanRevokeSpecification
{
    public bool IsSatisfiedBy(MandateStatus status)
    {
        return status == MandateStatus.Enforced;
    }
}
